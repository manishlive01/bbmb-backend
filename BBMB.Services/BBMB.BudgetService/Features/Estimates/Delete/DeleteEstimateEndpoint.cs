using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    public class DeleteEstimateEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/estimates/{workId:int}/{estimateCode}", async (int workId, string estimateCode, IMediator mediator) =>
            {
                var command = new DeleteEstimateCommand(workId, estimateCode);
                var response = await mediator.Send(command);

                return BudgetResults.From(response, _ => Results.NoContent());
            })
            .RequireAuthorization("BudgetRight:Legacy.BudgetEntry")
            .WithName("DeleteEstimate")
            .WithTags("Estimates")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
