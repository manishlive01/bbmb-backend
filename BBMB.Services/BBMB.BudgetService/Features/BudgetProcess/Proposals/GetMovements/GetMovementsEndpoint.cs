using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetMovements
{
    public sealed class GetMovementsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}/movements", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetMovementsQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetProposalMovements")
                .WithTags("BudgetProposals")
                .Produces<Response<List<MovementResponseDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
