using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public class GetSubMajorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/accounting-heads/sub-major/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetSubMajorHeadByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Sub-major head retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "sub_major_head_not_found", message = ex.Message });
                }
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetSubMajorHeadById")
            .WithTags("SubMajorHeads")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
