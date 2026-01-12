using BBMB.BudgetService.Features.Geographical.States.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints; 
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.GetList
{
    public class GetStateMasterListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-State-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetStateMasterListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetStateMasterListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get State List")
            .WithDescription("Returns list of states directly without response wrapper")
            .WithTags("StateMaster");
        }
    }
}
