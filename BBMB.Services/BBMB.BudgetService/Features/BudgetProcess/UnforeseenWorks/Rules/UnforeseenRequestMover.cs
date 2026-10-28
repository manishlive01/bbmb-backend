using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Rules
{
    /// <summary>
    /// The one path by which an unforeseen request moves through its status model (design §6.3). It validates
    /// the (action, current status) pair, the acting office (Division for SUBMIT/RESUBMIT, Circle for FORWARD
    /// and the AT_CIRCLE return, CE for the AT_CE return), the remark on a return, writes one T-49 movement and
    /// updates the T-48 status and holder, all in one audited scope. AUTHORISE is not here — it creates
    /// provisions and a work code, so it is its own slice (P4-12).
    /// </summary>
    public sealed class UnforeseenRequestMover(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
    {
        public async Task<Response> MoveAsync(long requestId, string action, string? remark, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail(BudgetMessages.M20);

            var request = await context.UnforeseenRequests.FirstOrDefaultAsync(r => r.UnforeseenRequestId == requestId, ct);
            if (request is null)
                return BudgetResponses.Fail(BudgetMessages.NotFound);

            var move = UnforeseenMoves.For(action, request.Status);
            if (move is null)
                return BudgetResponses.Fail(BudgetMessages.M19); // invalid (action, status) pair

            // Only the office currently holding the request may act on it.
            if (request.HolderOfficeId != actor.OfficeId)
                return BudgetResponses.Fail(BudgetMessages.M19);

            if (move.RemarkRequired && string.IsNullOrWhiteSpace(remark))
                return BudgetResponses.Fail(BudgetMessages.M12);

            var toOffice = await ResolveToOfficeAsync(request, action, actor.OfficeId, ct);
            if (toOffice is null)
                return BudgetResponses.Fail(BudgetMessages.M19);

            await using var tx = await scope.BeginAsync($"UNFORESEEN_{action}", ct: ct);
            context.UnforeseenRequestMovements.Add(new UnforeseenRequestMovement
            {
                UnforeseenRequestId = request.UnforeseenRequestId,
                FromStatus = move.FromStatus,
                ToStatus = move.ToStatus,
                FromOfficeId = actor.OfficeId,
                ToOfficeId = toOffice.Value,
                Remark = string.IsNullOrWhiteSpace(remark) ? null : remark.Trim(),
                RoleCode = actor.RoleCode,
                ClientIp = actor.ClientIp,
            });
            request.Status = move.ToStatus;
            request.HolderOfficeId = toOffice.Value;
            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new Response { IsSucced = true, Message = $"Request {action.ToLowerInvariant()} recorded" };
        }

        /// <summary>
        /// The office the request goes to: SUBMIT/RESUBMIT → the Division's Circle; FORWARD → the Circle's CE;
        /// a return → one level down (AT_CE → the Circle, AT_CIRCLE → the Division of the work). Null when the
        /// expected office does not exist in the hierarchy.
        /// </summary>
        private async Task<long?> ResolveToOfficeAsync(UnforeseenRequest request, string action, long actorOfficeId, CancellationToken ct)
        {
            switch (action)
            {
                case UnforeseenAction.Submit:
                case UnforeseenAction.Resubmit:
                    // From the Division (or the Circle after a return) up to the Circle of the Division office.
                    return await ParentOfAsync(request.DivisionOfficeId, OfficeLevel.Circle, ct);
                case UnforeseenAction.Forward:
                    // From the Circle up to its CE.
                    return await ParentOfAsync(actorOfficeId, OfficeLevel.Ce, ct);
                case UnforeseenAction.Return when request.Status == UnforeseenStatus.AtCe:
                    // CE returns to the Circle (one level down) = the Division's Circle.
                    return await ParentOfAsync(request.DivisionOfficeId, OfficeLevel.Circle, ct);
                case UnforeseenAction.Return when request.Status == UnforeseenStatus.AtCircle:
                    // Circle returns to the Division of the work.
                    return request.DivisionOfficeId;
                default:
                    return null;
            }
        }

        private async Task<long?> ParentOfAsync(long officeId, string expectedLevel, CancellationToken ct)
        {
            var parentId = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == officeId).Select(o => o.ParentOfficeId).FirstOrDefaultAsync(ct);
            if (parentId is null)
                return null;
            var parentLevel = await context.BudgetOffices.AsNoTracking()
                .Where(o => o.OfficeId == parentId.Value).Select(o => o.OfficeLevel).FirstOrDefaultAsync(ct);
            return parentLevel == expectedLevel ? parentId : null;
        }
    }
}
