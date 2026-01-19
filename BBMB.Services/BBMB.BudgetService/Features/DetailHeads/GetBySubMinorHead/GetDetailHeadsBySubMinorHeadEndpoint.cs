using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetBySubMinorHead
{
    public class GetDetailHeadsBySubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/detail-heads/by-sub-minor-head/{subMinorHeadCode}", async (string subMinorHeadCode, ISender sender) =>
            {
                var query = new GetDetailHeadsBySubMinorHeadQuery(subMinorHeadCode);
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDetailHeadsBySubMinorHead")
            .WithTags("DetailHeads")
            .Produces(200);
        }
    }
}
