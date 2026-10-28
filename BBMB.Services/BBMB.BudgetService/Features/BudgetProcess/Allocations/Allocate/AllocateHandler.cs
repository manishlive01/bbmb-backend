using BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.Allocate
{
    /// <summary>
    /// Design §8.6 POST allocations (F-19, BUD-179..189, BUD-285, BUD-286). One audited transaction, acting for
    /// the FA &amp; CAO (BUD-037). Order, nothing written on any refusal (BUD-186, all-or-nothing):
    /// <list type="number">
    /// <item>actor (M-20); cycle absent → NOT_FOUND; not OPEN → M-17;</item>
    /// <item>the minutes attachment is of evidence class (VR-10, M-16);</item>
    /// <item>the plan is computed (every proposal at the body's placement state, else M-19 with the outstanding
    /// list) and the recomputed counts equal the confirmed counts, else M-19 with the preview mismatch;</item>
    /// <item>the T-40 allocation row; one provision per planned holder with its ALLOCATION movement; for
    /// FULL_BOARD the new RBE provision supersedes the live BSC RBE of the same holder (BUD-285); a work code
    /// for each WORK provision whose work has none (BUD-187, BUD-286); then every proposal moves ALLOCATE_RBE /
    /// ALLOCATE_OBE. All in the one scope opened for the whole allocation.</item>
    /// </list>
    /// </summary>
    public sealed class AllocateHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, AllocationRules rules,
        Proposals.Rules.IProposalTransitionService transitions)
        : IRequestHandler<AllocateCommand, Response>
    {
        public async Task<Response> Handle(AllocateCommand request, CancellationToken ct)
        {
            var dto = request.Dto;
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var cycleStatus = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (cycleStatus is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (cycleStatus != CycleStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M17);

            // VR-10: the minutes attachment is of evidence class (M-16, BUD-180).
            var minutesIsEvidence = await context.BudgetAttachments.AsNoTracking().AnyAsync(a =>
                a.AttachmentId == dto.MinutesAttachmentId &&
                a.EvidenceClass == AttachmentEvidenceClass.Evidence, ct);
            if (!minutesIsEvidence)
                return BudgetResponses.Fail(BudgetMessages.M16);

            var computation = await rules.ComputeAsync(request.CycleId, dto.Body, ct);
            if (computation is null)
                return BudgetResponses.Fail(BudgetMessages.M19);

            // The preview recomputed must equal what the Chief Engineer confirmed, else the figures changed
            // since the preview (catalogue gap OI-13): tell the client the real cause.
            if (computation.LinesToActivate != dto.ConfirmLinesToActivate
                || computation.TotalToActivate != dto.ConfirmTotalToActivate)
            {
                return BudgetResponses.Fail(BudgetMessages.M19, new AllocationPreviewMismatchDto
                {
                    Expected = new PreviewCounts { Lines = computation.LinesToActivate, Total = computation.TotalToActivate },
                    Actual = new PreviewCounts { Lines = dto.ConfirmLinesToActivate, Total = dto.ConfirmTotalToActivate },
                });
            }

            var workCodesGenerated = new List<string>();
            long allocationId;

            await using (var tx = await scope.BeginAsync("ALLOCATE", onBehalfOfPost: "FA_CAO", ct: ct))
            {
                // (1) T-40 allocation row.
                var allocation = new Allocation
                {
                    CycleId = request.CycleId,
                    Body = dto.Body,
                    MinutesReference = dto.MinutesReference,
                    MinutesDate = dto.MinutesDate,
                    MinutesAttachmentId = dto.MinutesAttachmentId,
                    FaCaoApprovalRef = dto.FaCaoApprovalRef,
                    Remarks = dto.Remarks,
                    LinesActivated = computation.LinesToActivate,
                    TotalActivated = computation.TotalToActivate,
                    RoleCode = actor.RoleCode,
                    OnBehalfOfPost = "FA_CAO",
                };
                context.Allocations.Add(allocation);
                await context.SaveChangesAsync(ct);
                allocationId = allocation.AllocationId;

                // (2) One provision per planned holder, each tagged to this allocation.
                var created = new List<(PlannedProvision Plan, Provision Row)>();
                foreach (var plan in computation.Provisions)
                {
                    var row = new Provision
                    {
                        FinYear = plan.FinYear,
                        Basis = plan.Basis,
                        Category = plan.Category,
                        HolderType = plan.HolderType,
                        WorkId = plan.WorkId,
                        OfficeId = plan.OfficeId,
                        AccountCodeId = plan.AccountCodeId,
                        DemandLineId = plan.DemandLineId,
                        RecommendationId = plan.RecommendationId,
                        DistributionId = plan.DistributionId,
                        AllocationId = allocationId,
                        EffectiveFrom = plan.Basis == ProvisionBasis.Obe
                            ? new DateOnly(plan.FinYear - 1, 4, 1)   // OBE effective 1 April of fin_year (BUD-182)
                            : DateOnly.FromDateTime(DateTime.UtcNow), // BSC RBE effective today
                        BudgetaryCheck = plan.BudgetaryCheck,
                    };
                    context.Provisions.Add(row);
                    created.Add((plan, row));
                }
                await context.SaveChangesAsync(ct);

                // (3) FULL_BOARD: the new RBE provision supersedes the live BSC RBE of the same holder (BUD-285).
                if (dto.Body == AllocationBody.FullBoard)
                {
                    foreach (var (plan, row) in created.Where(c => c.Plan.Basis == ProvisionBasis.Rbe))
                    {
                        var superseded = await context.Provisions.FirstOrDefaultAsync(p =>
                            p.ProvisionId != row.ProvisionId &&
                            p.SupersededByProvisionId == null &&
                            p.Basis == ProvisionBasis.Rbe &&
                            p.FinYear == row.FinYear &&
                            p.HolderType == row.HolderType &&
                            p.WorkId == row.WorkId &&
                            p.OfficeId == row.OfficeId &&
                            p.AccountCodeId == row.AccountCodeId, ct);
                        if (superseded is not null)
                            superseded.SupersededByProvisionId = row.ProvisionId;
                    }
                    await context.SaveChangesAsync(ct);
                }

                // (4) One ALLOCATION movement per provision (positive; DB-20 total = sum of movements).
                foreach (var (plan, row) in created)
                    context.ProvisionMovements.Add(new ProvisionMovement
                    {
                        ProvisionId = row.ProvisionId,
                        MovementType = ProvisionMovementType.Allocation,
                        Amount = plan.Amount,
                        AllocationId = allocationId,
                    });
                await context.SaveChangesAsync(ct);

                // (5) A work code for each WORK provision whose work has none (BUD-187, BUD-286). The year is
                // the provision's year; fn_generate_work_code is the only writer and is a no-op for a coded work.
                foreach (var workId in computation.WorksToReceiveCode)
                {
                    var finYear = created.First(c => c.Plan.WorkId == workId).Plan.FinYear;
                    var code = await context.Database
                        .SqlQuery<string>($"SELECT budget.fn_generate_work_code({workId}, {AllocationRules.OccasionFor(dto.Body)}, {finYear}) AS \"Value\"")
                        .SingleAsync(ct);
                    if (!string.IsNullOrEmpty(code))
                        workCodesGenerated.Add(code);
                }

                await tx.CommitAsync(ct);
            }

            // (6) Move every proposal ALLOCATE_RBE / ALLOCATE_OBE, each one T-33 row (its own audited scope).
            var moved = await transitions.MoveAllAsync(
                computation.ProposalIds, AllocationRules.AllocateActionFor(dto.Body), null, ct);
            if (!moved.IsSucced)
                return moved;

            return new Response<AllocationResponseDto>
            {
                IsSucced = true,
                Message = "Allocation recorded",
                Data = new AllocationResponseDto
                {
                    AllocationId = allocationId,
                    LinesActivated = computation.LinesToActivate,
                    TotalActivated = computation.TotalToActivate,
                    WorkCodesGenerated = workCodesGenerated,
                },
            };
        }

    }
}
