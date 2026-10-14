using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead
{
    public class GetSubMinorHeadsByMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/sub-minor-heads/list/{minorHeadCode}", async (string minorHeadCode, IMediator mediator) =>
            {
                var query = new GetSubMinorHeadsByMinorHeadQuery(minorHeadCode);
                var result = await mediator.Send(query);
                return Results.Ok(new { message = "Sub-minor heads retrieved successfully", data = result });
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("GetSubMinorHeadsByMinorHead")
            .WithTags("SubMinorHeads")
            .Produces<List<GetSubMinorHeadsByMinorHeadResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
