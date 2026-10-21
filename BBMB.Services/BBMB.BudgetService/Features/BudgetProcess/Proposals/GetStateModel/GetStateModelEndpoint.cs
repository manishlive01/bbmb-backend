using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel
{
    public sealed class GetStateModelEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-states", async (IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetStateModelQuery(), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetStateModel")
                .WithTags("BudgetProposals")
                .Produces<Response<StateModelResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
