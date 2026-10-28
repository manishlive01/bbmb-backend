using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit.Dtos;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit
{
    /// <summary>
    /// Design §8.7 submit (F-21; VR-08, VR-10, BUD-202..209, BUD-284, BUD-291, BUD-309). Order, nothing written
    /// on refusal:
    /// <list type="number">
    /// <item>actor (M-20); entry absent → NOT_FOUND; not OPEN → M-19; confirm required (M-19);</item>
    /// <item>the order attachment is of evidence class (VR-10, M-16); the non-removed surrenders equal the
    /// additional amount (VR-08, M-18);</item>
    /// <item>one REAPPROP_SUBMIT scope: a new-work destination gets a WORK provision and a generated code
    /// (REAPPROPRIATION); a REAPPROP_IN movement on the destination provision (+additional) and a REAPPROP_OUT
    /// on each source provision (−amount), balanced (DB-21); the correction flags the move satisfies are
    /// cleared; a REALLOCATE_ESTIMATES flag is raised on the destination work (BUD-291); status SUBMITTED
    /// (DB-24).</item>
    /// </list>
    /// Right F21.Enter (endpoint).
    /// </summary>
    public sealed class SubmitReappropriationHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, ReappropriationRules rules)
        : IRequestHandler<SubmitReappropriationCommand, Response>
    {
        public async Task<Response> Handle(SubmitReappropriationCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            if (!request.Dto.Confirm)
                return BudgetResponses.Fail(BudgetMessages.M19);

            var entry = await context.Reappropriations.FirstOrDefaultAsync(r => r.ReappropriationId == request.Id, ct);
            if (entry is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (entry.Status != ReappropriationStatus.Open)
                return BudgetResponses.Fail(BudgetMessages.M19);

            // VR-10: the order attachment is of evidence class (M-16, BUD-202).
            if (!await context.BudgetAttachments.AsNoTracking().AnyAsync(a =>
                    a.AttachmentId == request.Dto.OrderAttachmentId && a.EvidenceClass == AttachmentEvidenceClass.Evidence, ct))
                return BudgetResponses.Fail(BudgetMessages.M16);

            // VR-08: the non-removed surrenders equal the additional amount (BUD-203).
            var sources = await context.ReappropriationSources.AsNoTracking()
                .Where(s => s.ReappropriationId == entry.ReappropriationId && !s.IsRemoved)
                .Select(s => new { s.ProvisionId, s.Amount }).ToListAsync(ct);
            if (sources.Count == 0 || sources.Sum(s => s.Amount) != entry.AdditionalAmount)
                return BudgetResponses.Fail(BudgetMessages.M18);

            string? generatedCode = null;

            await using (var tx = await scope.BeginAsync("REAPPROP_SUBMIT", ct: ct))
            {
                // Resolve (or open) the destination provision.
                long destinationProvisionId;
                if (entry.DestinationProvisionId is { } existing)
                {
                    destinationProvisionId = existing;
                }
                else
                {
                    // New-work destination (BUD-282..284): open a WORK provision under the work's head and
                    // generate its code. Take the basis/category/office from a source provision's shape.
                    var newWorkId = entry.DestinationNewWorkId!.Value;
                    var work = await context.Works.AsNoTracking()
                        .Where(w => w.Id == newWorkId)
                        .Select(w => new { w.DivisionId, w.MajorAccCode, w.SubMajorAccCode, w.MinorAccCode, w.SubMinorAccCode, w.DetailAccCode, w.ObjectAccCode })
                        .SingleAsync(ct);
                    var divisionOfficeId = await context.BudgetOffices.AsNoTracking()
                        .Where(o => o.DivisionId == work.DivisionId && o.OfficeLevel == Offices.Entities.OfficeLevel.Division)
                        .Select(o => o.OfficeId).FirstAsync(ct);
                    var accountCodeId = await context.HeadOfAccounts.AsNoTracking()
                        .Where(h => h.MajorAccCode == work.MajorAccCode && (h.SubMajorAccCode ?? "") == work.SubMajorAccCode &&
                                    h.MinorAccCode == work.MinorAccCode && (h.SubMinorAccCode ?? "") == work.SubMinorAccCode &&
                                    h.DetailAccCode == work.DetailAccCode && h.ObjectAccCode == work.ObjectAccCode)
                        .Select(h => (long?)h.AccountCodeId).FirstOrDefaultAsync(ct);
                    // The destination provision mirrors a source provision's basis and category (RBE, same year).
                    var sample = await context.Provisions.AsNoTracking()
                        .Where(p => p.ProvisionId == sources[0].ProvisionId)
                        .Select(p => new { p.Basis, p.Category }).SingleAsync(ct);
                    var destination = new Provision
                    {
                        FinYear = entry.FinYear,
                        Basis = sample.Basis,
                        Category = sample.Category,
                        HolderType = ProvisionHolderType.Work,
                        WorkId = newWorkId,
                        OfficeId = divisionOfficeId,
                        AccountCodeId = accountCodeId,
                        EffectiveFrom = sample.Basis == ProvisionBasis.Obe
                            ? new DateOnly(entry.FinYear - 1, 4, 1)
                            : DateOnly.FromDateTime(DateTime.UtcNow),
                        BudgetaryCheck = sample.Category != Proposals.ProposalCategory.Receipts,
                    };
                    context.Provisions.Add(destination);
                    await context.SaveChangesAsync(ct);
                    destinationProvisionId = destination.ProvisionId;

                    generatedCode = await context.Database
                        .SqlQuery<string>($"SELECT budget.fn_generate_work_code({newWorkId}, {WorkCodeOccasion.Reappropriation}, {entry.FinYear}) AS \"Value\"")
                        .SingleAsync(ct);
                    entry.DestinationProvisionId = destinationProvisionId;
                }

                // Movements: a REAPPROP_IN on the destination (+additional), a REAPPROP_OUT on each source
                // (−amount), tagged to this re-appropriation; balanced (DB-21).
                context.ProvisionMovements.Add(new ProvisionMovement
                {
                    ProvisionId = destinationProvisionId,
                    MovementType = ProvisionMovementType.ReappropIn,
                    Amount = entry.AdditionalAmount,
                    ReappropriationId = entry.ReappropriationId,
                });
                foreach (var s in sources)
                    context.ProvisionMovements.Add(new ProvisionMovement
                    {
                        ProvisionId = s.ProvisionId,
                        MovementType = ProvisionMovementType.ReappropOut,
                        Amount = -s.Amount,
                        ReappropriationId = entry.ReappropriationId,
                    });

                // Clear the correction flags this move satisfies (BUD-309): open flags on the destination
                // provision or its work.
                var destWorkId = entry.DestinationNewWorkId
                    ?? await context.Provisions.AsNoTracking().Where(p => p.ProvisionId == destinationProvisionId)
                        .Select(p => p.WorkId).FirstOrDefaultAsync(ct);
                var flags = await context.CorrectionFlags
                    .Where(f => f.ClearedAt == null &&
                                (f.ProvisionId == destinationProvisionId || (destWorkId != null && f.WorkId == destWorkId)))
                    .ToListAsync(ct);
                foreach (var f in flags)
                {
                    f.ClearedAt = DateTimeOffset.UtcNow;
                    f.ClearedBy = actor.EmployeeId;
                    f.ClearedByReappropriationId = entry.ReappropriationId;
                }

                // Mark the destination's estimate allocation for reallocation (BUD-291), where it is a work.
                if (destWorkId is { } wid && !flags.Any(f => f.Reason == CorrectionFlagReason.ReallocateEstimates && f.WorkId == wid))
                    context.CorrectionFlags.Add(new CorrectionFlag
                    {
                        Reason = CorrectionFlagReason.ReallocateEstimates,
                        WorkId = wid,
                        ProvisionId = destinationProvisionId,
                        IsCritical = false,
                    });

                entry.Status = ReappropriationStatus.Submitted;
                entry.OrderAttachmentId = request.Dto.OrderAttachmentId;
                entry.ClosedAt = DateTimeOffset.UtcNow;
                entry.ClosedBy = actor.EmployeeId;
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }

            return new Response<SubmitReappropriationResponseDto>
            {
                IsSucced = true,
                Message = "Re-appropriation submitted",
                Data = new SubmitReappropriationResponseDto { WorkCodeGenerated = generatedCode },
            };
        }
    }
}
