using BBMB.BudgetService.Features.Estimates.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Estimates.GetById
{
    public static class GetEstimateByIdEndpoint
    {
        public static void MapGetEstimateByIdEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/estimates/{workCode}/{estimateCode}", async (
                [FromRoute] string workCode,
                [FromRoute] string estimateCode,
                [FromServices] IMediator mediator) =>
            {
                var query = new GetEstimateByIdQuery(workCode, estimateCode);
                var result = await mediator.Send(query);

                if (!result.IsSucced)
                {
                    return Results.NotFound(result);
                }

                return Results.Ok(result);
            })
            .WithName("GetEstimateById")
            .WithTags("Estimates")
            .Produces<Response<GetEstimateByIdResponseDto>>(200)
            .Produces<Response<GetEstimateByIdResponseDto>>(404);
        }
    }
}
