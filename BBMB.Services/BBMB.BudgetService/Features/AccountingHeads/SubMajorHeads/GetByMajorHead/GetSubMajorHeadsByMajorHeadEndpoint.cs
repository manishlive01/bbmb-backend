using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetByMajorHead
{
    public class GetSubMajorHeadsByMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/accounting-heads/sub-major/by-major-head/{majorHeadCode}", async (string majorHeadCode, IMediator mediator) =>
            {
                var query = new GetSubMajorHeadsByMajorHeadQuery(majorHeadCode);
                var result = await mediator.Send(query);
                return Results.Ok(new { message = "Sub-major heads retrieved successfully", data = result });
            })
            .WithName("GetSubMajorHeadsByMajorHead")
            .WithTags("SubMajorHeads")
            .Produces(StatusCodes.Status200OK);
        }
    }
}
