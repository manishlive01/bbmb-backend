using BBMB_Budget.Features.Geographical.States.GetList.Dtos;
using BBMB_Budget.Shared.Endpoints;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.GetList
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
