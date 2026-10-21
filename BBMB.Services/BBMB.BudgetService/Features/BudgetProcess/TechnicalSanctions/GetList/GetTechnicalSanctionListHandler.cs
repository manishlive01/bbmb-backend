using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetList
{
    /// <summary>Sanctions of the works the actor sees, each with every version and its estimate code.</summary>
    public sealed class GetTechnicalSanctionListHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetTechnicalSanctionListQuery, Response<List<TechnicalSanctionDto>>>
    {
        public async Task<Response<List<TechnicalSanctionDto>>> Handle(GetTechnicalSanctionListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<TechnicalSanctionDto>>(BudgetMessages.M20);
            var visible = await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct);
            return new Response<List<TechnicalSanctionDto>>
            {
                IsSucced = true, Message = "Technical sanctions",
                Data = await TechnicalSanctionReads.LoadAsync(context, visible.ToList(), null, request.WorkId, ct),
            };
        }
    }
}
