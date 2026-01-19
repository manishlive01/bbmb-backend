using BBMB.BudgetService.Features.MinorHeads.SubMinor.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using FluentValidation;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Update
{
    public class UpdateSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/minor-heads/sub-minor/{code}", async (string code, UpdateSubMinorHeadDto dto, IMediator mediator, IValidator<UpdateSubMinorHeadDto> validator) =>
            {
                var validationResult = await validator.ValidateAsync(dto);
                if (!validationResult.IsValid)
                {
                    return Results.BadRequest(new { errors = validationResult.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }) });
                }

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
