using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById
{
    public class GetMinorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/minor-heads/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetMinorHeadByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Minor head retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "minor_head_not_found", message = ex.Message });
                }
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetMinorHeadById")
            .WithTags("MinorHeads")
            .Produces<GetMinorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
