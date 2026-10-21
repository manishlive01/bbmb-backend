using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements
{
    /// <summary>Design §8.3 movements (BUD-238): the T-33 rows of a visible proposal in the order written. Scoped.</summary>
    public sealed class GetMovementsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetMovementsQuery, Response<List<MovementResponseDto>>>
    {
        public async Task<Response<List<MovementResponseDto>>> Handle(GetMovementsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<MovementResponseDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<List<MovementResponseDto>>(BudgetMessages.NotFound);
            var items = await context.ProposalMovements.AsNoTracking()
                .Where(m => m.ProposalId == p.ProposalId).OrderBy(m => m.MovementId)
                .Select(m => new MovementResponseDto
                {
                    MovementId = m.MovementId, FromState = m.FromState, ActionCode = m.ActionCode, ToState = m.ToState,
                    FromOfficeId = m.FromOfficeId, ToOfficeId = m.ToOfficeId, Remark = m.Remark, RoleCode = m.RoleCode,
                    OnBehalfOfPost = m.OnBehalfOfPost, OfficerId = m.CreatedBy, ChargeId = m.CreatedChargeId, MovedAt = m.CreatedAt,
                }).ToListAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, items.SelectMany(i => new[] { i.FromOfficeId, i.ToOfficeId }), ct);
            foreach (var i in items)
            {
                i.FromOfficeName = names.GetValueOrDefault(i.FromOfficeId);
                i.ToOfficeName = names.GetValueOrDefault(i.ToOfficeId);
            }
            return new Response<List<MovementResponseDto>> { IsSucced = true, Message = "Movements retrieved successfully", Data = items };
        }
    }
}
