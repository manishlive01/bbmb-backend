using BBMB.BudgetService.Features.MinorHeads.SubMinor.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.GetById
{
    public class GetSubMinorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/minor-heads/sub-minor/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetSubMinorHeadByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Sub-minor head retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "sub_minor_head_not_found", message = ex.Message });
                }
            })
            .WithName("GetSubMinorHeadById")
            .WithTags("SubMinorHeads")
            .Produces<GetSubMinorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
