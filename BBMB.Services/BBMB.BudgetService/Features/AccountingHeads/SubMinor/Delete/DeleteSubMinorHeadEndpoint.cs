using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Delete
{
    public class DeleteSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/sub-minor-heads/{code}", async (string code, IMediator mediator) =>
            {
                var command = new DeleteSubMinorHeadCommand(code);
                var result = await mediator.Send(command);

                if (!result.IsSucced)
                {
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(new { error = "sub_minor_head_not_found", message = result.Message });
                    
                    if (result.Message.Contains("dependent") || result.Message.Contains("constraint"))
                        return Results.Conflict(new { error = "has_dependencies", message = result.Message });
                    
                    return Results.BadRequest(new { error = "delete_failed", message = result.Message });
                }

                return Results.NoContent();
            })
            .WithName("DeleteSubMinorHead")
            .WithTags("SubMinorHeads")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
