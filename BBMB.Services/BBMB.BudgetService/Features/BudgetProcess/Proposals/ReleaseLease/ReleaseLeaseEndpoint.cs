using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReleaseLease
{
    public sealed class ReleaseLeaseEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/budget-proposals/{id:long}/lease", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new ReleaseLeaseCommand(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("ReleaseLease")
                .WithTags("BudgetProposals")
                .Produces<Response<LeaseResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<LeaseResponseDto>>(StatusCodes.Status409Conflict);
        }
    }
}
