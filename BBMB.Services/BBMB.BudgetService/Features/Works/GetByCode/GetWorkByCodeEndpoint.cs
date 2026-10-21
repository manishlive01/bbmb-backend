using BBMB.BudgetService.Features.Works.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GetByCode
{
    // P4 (design §8.6): GET /api/works/by-code/{workCode}. The string code is either form.
    public class GetWorkByCodeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/works/by-code/{workCode}", async (string workCode, IMediator mediator) =>
            {
                try
                {
                    var result = await mediator.Send(new GetWorkByCodeQuery(workCode));
                    return Results.Ok(new { message = "Work retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "work_not_found", message = ex.Message });
                }
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetWorkByCode")
            .WithTags("Works")
            .Produces<GetWorkByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
