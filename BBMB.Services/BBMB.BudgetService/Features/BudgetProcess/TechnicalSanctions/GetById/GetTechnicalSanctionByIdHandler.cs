using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetById
{
    /// <summary>One sanction with every version; absent or outside the actor's offices → NOT_FOUND (§4.9).</summary>
    public sealed class GetTechnicalSanctionByIdHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetTechnicalSanctionByIdQuery, Response<TechnicalSanctionDto>>
    {
        public async Task<Response<TechnicalSanctionDto>> Handle(GetTechnicalSanctionByIdQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<TechnicalSanctionDto>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            var found = (await TechnicalSanctionReads.LoadAsync(context, visible.ToList(), request.TechnicalSanctionId, null, ct)).SingleOrDefault();
            return found is null
                ? BudgetResponses.Fail<TechnicalSanctionDto>(BudgetMessages.NotFound)
                : new Response<TechnicalSanctionDto> { IsSucced = true, Message = "Technical sanction", Data = found };
        }
    }
}
