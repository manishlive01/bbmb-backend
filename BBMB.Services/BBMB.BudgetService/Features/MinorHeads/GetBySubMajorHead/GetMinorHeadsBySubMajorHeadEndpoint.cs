using BBMB.BudgetService.Features.MinorHeads.GetBySubMajorHead.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.GetBySubMajorHead
{
    public class GetMinorHeadsBySubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/minor-heads/by-sub-major-head/{subMajorHeadCode}", async (string subMajorHeadCode, IMediator mediator) =>
            {
                var query = new GetMinorHeadsBySubMajorHeadQuery(subMajorHeadCode);
                var result = await mediator.Send(query);
                return Results.Ok(new { message = "Minor heads retrieved successfully", data = result });
            })
            .WithName("GetMinorHeadsBySubMajorHead")
            .WithTags("MinorHeads")
            .Produces<List<GetMinorHeadsBySubMajorHeadResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
