using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.AcquireLease
{
    public sealed class AcquireLeaseEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Budget.View at the endpoint; the state's write right is authorised in the handler (design §4.10).
            app.MapPost("/api/budget-proposals/{id:long}/lease", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new AcquireLeaseCommand(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("AcquireLease")
                .WithTags("BudgetProposals")
                .Produces<Response<LeaseResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<LeaseResponseDto>>(StatusCodes.Status409Conflict);
        }
    }
}
