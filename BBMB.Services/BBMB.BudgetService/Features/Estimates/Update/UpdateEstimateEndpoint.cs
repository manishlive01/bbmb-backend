using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public static class UpdateEstimateEndpoint
    {
        public static void MapUpdateEstimateEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/estimates/{workId:int}/{estimateCode}", async (
                [FromRoute] int workId,
                [FromRoute] string estimateCode,
                [FromBody] UpdateEstimateDto dto,
                [FromServices] IMediator mediator) =>
            {
                var command = new UpdateEstimateCommand(workId, estimateCode, dto);
                var result = await mediator.Send(command);
                return BudgetResults.Ok(result);
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
