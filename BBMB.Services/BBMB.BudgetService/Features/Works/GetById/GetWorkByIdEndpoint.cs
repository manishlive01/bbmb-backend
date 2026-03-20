using BBMB.BudgetService.Features.Works.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetById
{
    public class GetWorkByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/works/{code}", async (int code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetWorkByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Work retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "work_not_found", message = ex.Message });
                }
            })
            .WithName("GetWorkById")
            .WithTags("Works")
            .Produces<GetWorkByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
