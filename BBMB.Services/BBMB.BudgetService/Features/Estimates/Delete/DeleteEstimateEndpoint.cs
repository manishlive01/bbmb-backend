using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    public class DeleteEstimateEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/estimates/{workCode}/{estimateCode}", async (int workCode, string estimateCode, IMediator mediator) =>
            {
                var command = new DeleteEstimateCommand(workCode, estimateCode);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.NoContent()
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:Legacy.BudgetEntry")
            .WithName("DeleteEstimate")
            .WithTags("Estimates")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
