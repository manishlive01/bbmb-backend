using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Delete
{
    public class DeleteMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/Delete-MajorHead", async (
                [FromBody] GetByStringIdRequestDto dto,
                [FromServices] IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new DeleteMajorHeadCommand(dto);

                var response = await mediator.Send(command, ct);

                if (!response.IsSucced)
                {
                    if (response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                        return Results.NotFound(response);

                    return Results.Conflict(response);
                }

                return Results.NoContent();
            })
            .WithName("Budget.MajorHead.Delete")
            .WithTags("MajorHead")
            .WithSummary("Delete Major Head")
            .WithDescription("Deletes a Major Head if no Sub Major Heads are linked")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
