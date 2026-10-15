using System.Text.Json;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise
{
    /// <summary>
    /// Design §8.6 authorise (F-30, DDD T-50; BUD-295..300). The module's one authorisation. Order, nothing
    /// written on refusal:
    /// <list type="number">
    /// <item>actor (M-20); request absent → NOT_FOUND; status not AT_CE → M-19;</item>
    /// <item>the actor is the CE of the Division's charge (BUD-295), else M-20;</item>
    /// <item>VR-13: each amount ≤ the balance of the provision drawn upon (M-29); VR-04: the total ≤ the
    /// ceiling of the work's approval (M-09, BUD-299);</item>
    /// <item>one UNFORESEEN_AUTHORISE scope: the work code is generated (UNFORESEEN_AUTH); the T-50 row with the
    /// request snapshot and the code; a WORK provision per year drawn upon, under the approval's head, at the
    /// Division office; an UNFORESEEN_OUT (on the unforeseen provision) and an UNFORESEEN_IN (on the new WORK
    /// provision) movement per year, balanced (DB-21); the request moves to AUTHORISED.</item>
    /// </list>
    /// Right F30.Authorise (endpoint).
    /// </summary>
    public sealed class AuthoriseUnforeseenHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope, UnforeseenRules rules)
        : IRequestHandler<AuthoriseUnforeseenCommand, Response>
    {
        public async Task<Response> Handle(AuthoriseUnforeseenCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var req = await context.UnforeseenRequests.FirstOrDefaultAsync(r => r.UnforeseenRequestId == request.Id, ct);
            if (req is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);
            if (req.Status != UnforeseenStatus.AtCe)
                return BudgetResponses.Fail(BudgetMessages.M19);

            if (!await rules.ActorIsCeOfChargeAsync(actor.OfficeId, req.DivisionOfficeId, ct))
                return BudgetResponses.Fail(BudgetMessages.M20);

            // VR-13 (M-29): neither amount above its provision's available balance.
            if (!await rules.Vr13HoldsAsync(req.CurrentProvisionId, req.AmountCurrent, ct) ||
                !await rules.Vr13HoldsAsync(req.NextProvisionId, req.AmountNext, ct))
                return BudgetResponses.Fail(BudgetMessages.M29);

            // VR-04 (M-09): the total authorised within the work's approval ceiling (BUD-299).
            var total = (req.AmountCurrent ?? 0) + (req.AmountNext ?? 0);
            if (!await rules.Vr04HoldsAsync(req.WorkId, total, ct))
                return BudgetResponses.Fail(BudgetMessages.M09);

            // The approval's head (account_code_id) of the work's current AA version — the head the WORK
            // provision takes (BUD-298).
            var approvalHead = await (
                from a in context.AdminApprovals.AsNoTracking()
                where a.WorkId == req.WorkId
                join v in context.AdminApprovalVersions.AsNoTracking()
                    on new { a.AdminApprovalId, Ver = a.CurrentVersionNo } equals new { v.AdminApprovalId, Ver = v.VersionNo }
                select new { a.AdminApprovalId, v.VersionNo, v.AccountCodeId, v.AmountSanctioned }).FirstOrDefaultAsync(ct);
            if (approvalHead is null)
                return BudgetResponses.Fail(BudgetMessages.M19);

            // Each year drawn upon: the unforeseen provision, the amount, and the WORK provision's year/category.
            var draws = new List<(long UnforeseenProvisionId, decimal Amount)>();
            if (req.CurrentProvisionId is { } cp && req.AmountCurrent is { } ca) draws.Add((cp, ca));
            if (req.NextProvisionId is { } np && req.AmountNext is { } na) draws.Add((np, na));

            var unforeseenProvisions = await context.Provisions.AsNoTracking()
                .Where(p => draws.Select(d => d.UnforeseenProvisionId).Contains(p.ProvisionId))
                .Select(p => new { p.ProvisionId, p.FinYear, p.Basis, p.Category, p.OfficeId }).ToListAsync(ct);

            string generatedCode;
            await using (var tx = await scope.BeginAsync("UNFORESEEN_AUTHORISE", ct: ct))
            {
                // (1) Generate the work code (UNFORESEEN_AUTH). The year is the current-year draw's year, else
                // the single draw's year (BUD-298).
                var codeYear = unforeseenProvisions.Min(p => p.FinYear);
                generatedCode = await context.Database
                    .SqlQuery<string>($"SELECT budget.fn_generate_work_code({req.WorkId}, {WorkCodeOccasion.UnforeseenAuth}, {codeYear}) AS \"Value\"")
                    .SingleAsync(ct);

                // (2) T-50 authorisation with the request snapshot and the generated code.
                var snapshot = JsonSerializer.Serialize(new
                {
                    req.UnforeseenRequestId, req.WorkId, req.DivisionOfficeId,
                    req.CurrentProvisionId, req.AmountCurrent, req.NextProvisionId, req.AmountNext,
                    Approval = new { approvalHead.AdminApprovalId, approvalHead.VersionNo, approvalHead.AmountSanctioned },
                });
                var authorisation = new UnforeseenAuthorisation
                {
                    UnforeseenRequestId = req.UnforeseenRequestId,
                    CeOfficeId = actor.OfficeId,
                    AmountCurrent = req.AmountCurrent,
                    AmountNext = req.AmountNext,
                    Reasons = request.Dto.Reasons,
                    RequestSnapshot = snapshot,
                    WorkCodeGenerated = generatedCode,
                    RoleCode = actor.RoleCode,
                    ClientIp = actor.ClientIp,
                };
                context.UnforeseenAuthorisations.Add(authorisation);
                await context.SaveChangesAsync(ct);

                // (3) A WORK provision per year drawn upon, under the approval's head, at the Division office;
                //     with the balanced UNFORESEEN_OUT (on the unforeseen provision) / UNFORESEEN_IN (on the
                //     new WORK provision) movements (DB-21).
                foreach (var (unforeseenProvisionId, amount) in draws)
                {
                    var up = unforeseenProvisions.Single(p => p.ProvisionId == unforeseenProvisionId);
                    var workProvision = new Provision
                    {
                        FinYear = up.FinYear,
                        Basis = up.Basis,
                        Category = up.Category,
                        HolderType = ProvisionHolderType.Work,
                        WorkId = req.WorkId,
                        OfficeId = req.DivisionOfficeId,
                        AccountCodeId = approvalHead.AccountCodeId,
                        EffectiveFrom = up.Basis == ProvisionBasis.Obe
                            ? new DateOnly(up.FinYear - 1, 4, 1)
                            : DateOnly.FromDateTime(DateTime.UtcNow),
                        BudgetaryCheck = up.Category != Proposals.ProposalCategory.Receipts,
                    };
                    context.Provisions.Add(workProvision);
                    await context.SaveChangesAsync(ct);

                    context.ProvisionMovements.Add(new ProvisionMovement
                    {
                        ProvisionId = unforeseenProvisionId,
                        MovementType = ProvisionMovementType.UnforeseenOut,
                        Amount = -amount,
                        UnforeseenAuthorisationId = authorisation.UnforeseenAuthorisationId,
                    });
                    context.ProvisionMovements.Add(new ProvisionMovement
                    {
                        ProvisionId = workProvision.ProvisionId,
                        MovementType = ProvisionMovementType.UnforeseenIn,
                        Amount = amount,
                        UnforeseenAuthorisationId = authorisation.UnforeseenAuthorisationId,
                    });
                    await context.SaveChangesAsync(ct);
                }

                // (4) The request is authorised.
                req.Status = UnforeseenStatus.Authorised;
                req.HolderOfficeId = actor.OfficeId;
                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
            }

            return new Response<AuthoriseUnforeseenResponseDto>
            {
                IsSucced = true,
                Message = "Unforeseen work authorised",
                Data = new AuthoriseUnforeseenResponseDto { WorkCode = generatedCode },
            };
        }
    }
}
