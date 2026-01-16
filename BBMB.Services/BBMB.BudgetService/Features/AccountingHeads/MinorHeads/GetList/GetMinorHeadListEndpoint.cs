using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public class GetMinorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-MinorHead-List", async (
                IMediator mediator) =>
            {
                var response = await mediator.Send(
                    new GetMinorHeadListCommand());

                return Results.Ok(response);
            })
            .Produces<List<GetMinorHeadListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Minor Head List")
            .WithDescription("Gets the list of all Minor Heads")
            .WithName("Get Minor Head List")
            .WithTags("MinorHead");
        }
    }
}
