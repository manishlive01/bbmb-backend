using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using FluentValidation;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Update
{
    public class UpdateSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/sub-minor-heads/{code}", async (string code, UpdateSubMinorHeadDto dto, IMediator mediator) =>
            {
                

                var command = new UpdateSubMinorHeadCommand(code, dto);
                var result = await mediator.Send(command);

                if (!result.IsSucced)
                {
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(new { error = "sub_minor_head_not_found", message = result.Message });
                    
                    return Results.BadRequest(new { error = "update_failed", message = result.Message });
                }

                return Results.Ok(new { message = result.Message });
            })
            .WithName("UpdateSubMinorHead")
            .WithTags("SubMinorHeads")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
