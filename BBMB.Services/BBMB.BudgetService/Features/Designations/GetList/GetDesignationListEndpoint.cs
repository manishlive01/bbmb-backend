using BBMB.BudgetService.Features.Designations.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Designations.GetList
{
    public class GetDesignationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/designations/list", async (
                [FromBody] GetDesignationListRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetDesignationListCommand(dto);
                var result = await mediator.Send(query, ct);
                return Results.Ok(result);
            })
            .WithName("GetDesignationList")
            .WithTags("Designations")
            .WithSummary("Get Designation List")
            .WithDescription("Retrieves designation list using body parameters with pagination, sorting, and filters")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
