using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetList
{
    public class GetBudgetTypeMasterListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-Budget-Type-Master-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetBudgetTypeMasterListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetBudgetTypeMasterListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Budget Type Master List")
            .WithDescription("Returns list of Budget Type Master directly without response wrapper")
            .WithTags("BudgetTypeMaster");
        }
    }
}
