using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetById
{
    public class GetDetailHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/detail-heads/{code}", async (string code, ISender sender) =>
            {
                try
                {
                    var query = new GetDetailHeadByIdCommand(code);
                    var result = await sender.Send(query);
                    return Results.Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetDetailHeadById")
            .WithTags("DetailHeads")
            .Produces(200)
            .Produces(404);
        }
    }
}
