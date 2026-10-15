using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById
{
    /// <summary>Design §8.3: header, state, holder, lease and availableActions; out of scope → NOT_FOUND.</summary>
    public sealed class GetProposalByIdHandler(
        BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope,
        IProposalTransitionService transitions, TimeProvider? clock = null)
        : IRequestHandler<GetProposalByIdQuery, Response<GetProposalByIdResponseDto>>
    {
        public async Task<Response<GetProposalByIdResponseDto>> Handle(GetProposalByIdQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            var p = actor is null ? null : await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<GetProposalByIdResponseDto>(BudgetMessages.NotFound);

            var state = await context.ProposalStates.AsNoTracking().SingleAsync(s => s.StateCode == p.StateCode, ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, [p.OriginOfficeId, p.HolderOfficeId], ct);
            var now = (clock ?? TimeProvider.System).GetUtcNow();
            var live = p.EditLeaseBy is not null && p.EditLeaseExpiresAt > now;
            return new Response<GetProposalByIdResponseDto>
            {
                IsSucced = true,
                Message = "Proposal retrieved",
                Data = new GetProposalByIdResponseDto
                {
                    ProposalId = p.ProposalId, CycleId = p.CycleId, Category = p.Category,
                    OriginOfficeId = p.OriginOfficeId, OriginOfficeName = names.GetValueOrDefault(p.OriginOfficeId),
                    StateCode = p.StateCode, StateName = state.StateName, FiguresOpen = state.FiguresOpen,
                    HolderOfficeId = p.HolderOfficeId, HolderOfficeName = names.GetValueOrDefault(p.HolderOfficeId),
                    FirstSubmittedAt = p.FirstSubmittedAt,
                    LeaseHeldBy = live ? p.EditLeaseBy : null, LeaseExpiresAt = live ? p.EditLeaseExpiresAt : null,
                    AvailableActions = await transitions.GetAvailableActionsAsync(p.ProposalId, ct),
                },
            };
        }
    }
}
