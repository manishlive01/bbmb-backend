using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetAssignableHeads
{
    /// <summary>
    /// The heads a head-wise line of the proposal may take (VR-06 at selection; design §8.4): live assignments of the
    /// proposal's origin office, not marked not for use. Proposal absent or not visible → NOT_FOUND (§4.9).
    /// </summary>
    public sealed class GetAssignableHeadsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetAssignableHeadsQuery, Response<List<AssignedHeadDto>>>
    {
        public async Task<Response<List<AssignedHeadDto>>> Handle(GetAssignableHeadsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AssignedHeadDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<List<AssignedHeadDto>>(BudgetMessages.NotFound);
            return new Response<List<AssignedHeadDto>>
            {
                IsSucced = true, Message = "Assignable heads",
                Data = p.Category == ProposalCategory.Works ? [] : await AssignedHeadReads.LiveAsync(context, p.OriginOfficeId, usableOnly: true, ct),
            };
        }
    }
}
