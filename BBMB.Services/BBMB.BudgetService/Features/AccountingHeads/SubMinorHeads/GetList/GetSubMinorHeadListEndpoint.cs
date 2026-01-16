using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList
{
    public class GetSubMinorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-SubMinorHead-List", async (
                IMediator mediator) =>
            {
                var response = await mediator.Send(
                    new GetSubMinorHeadListCommand());

                return Results.Ok(response);
            })
            .Produces<List<GetSubMinorHeadListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Sub Minor Head List")
            .WithDescription("Gets the list of all Sub Minor Heads")
            .WithName("Get Sub Minor Head List")
            .WithTags("SubMinorHead");
        }
    }
}
