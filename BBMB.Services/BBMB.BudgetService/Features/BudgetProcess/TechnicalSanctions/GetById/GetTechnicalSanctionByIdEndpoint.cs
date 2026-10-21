using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetById
{
    public sealed class GetTechnicalSanctionByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/technical-sanctions/{id:long}",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetTechnicalSanctionByIdQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetTechnicalSanctionById")
                .WithTags("TechnicalSanctions")
                .Produces<Response<TechnicalSanctionDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
