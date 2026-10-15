using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList
{
    /// <summary>
    /// The live heads of an office (design §8.4 assignment reads). Scoped (§4.9): an office outside the actor's visible
    /// offices is NOT_FOUND, so a DDO sees only its own heads. Not-for-use heads are listed with the mark.
    /// </summary>
    public sealed class GetHeadAssignmentsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetHeadAssignmentsQuery, Response<List<AssignedHeadDto>>>
    {
        public async Task<Response<List<AssignedHeadDto>>> Handle(GetHeadAssignmentsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AssignedHeadDto>>(BudgetMessages.M20);
            var officeId = request.OfficeId ?? actor.OfficeId;
            if (!(await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).Contains(officeId))
                return BudgetResponses.Fail<List<AssignedHeadDto>>(BudgetMessages.NotFound);
            return new Response<List<AssignedHeadDto>>
            {
                IsSucced = true, Message = "Assigned heads",
                Data = await AssignedHeadReads.LiveAsync(context, officeId, usableOnly: false, ct),
            };
        }
    }
}
