using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Delete
{
    public class DeleteSubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/accounting-heads/sub-major/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteSubMajorHeadCommand(code);
                var response = await mediator.Send(command);
                
                if (!response.IsSucced)
                {
                    if (response.Message.Contains("not found"))
                        return Results.NotFound(response);
                    else
                        return Results.Conflict(response);
                }
                
                return Results.NoContent();
            })
            .WithName("DeleteSubMajorHead")
            .WithTags("SubMajorHeads")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
