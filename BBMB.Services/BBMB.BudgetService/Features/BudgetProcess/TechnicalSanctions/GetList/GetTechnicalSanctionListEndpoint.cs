using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetList
{
    public sealed class GetTechnicalSanctionListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: the work filter travels in the body, not the URL (security requirement).
            app.MapPost("/api/technical-sanctions/list",
                    async (WorkFilterRequestDto? dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetTechnicalSanctionListQuery(dto?.WorkId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetTechnicalSanctions")
                .WithTags("TechnicalSanctions")
                .Produces<Response<List<TechnicalSanctionDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
