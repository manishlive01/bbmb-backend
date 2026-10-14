using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public static class UpdateEstimateEndpoint
    {
        public static void MapUpdateEstimateEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/estimates/{workCode}/{estimateCode}", async (
                [FromRoute] int workCode,
                [FromRoute] string estimateCode,
                [FromBody] UpdateEstimateDto dto,
                [FromServices] IMediator mediator) =>
            {
                var command = new UpdateEstimateCommand(workCode, estimateCode, dto);
                var result = await mediator.Send(command);

                if (!result.IsSucced)
                {
                    if (result.Message.Contains("not found"))
                    {
                        return Results.NotFound(result);
                    }
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .RequireAuthorization("BudgetRight:Legacy.BudgetEntry")
            .WithName("UpdateEstimate")
            .WithTags("Estimates")
            .Produces<Response<string>>(200)
            .Produces<Response<string>>(400)
            .Produces<Response<string>>(404);
        }
    }
}
