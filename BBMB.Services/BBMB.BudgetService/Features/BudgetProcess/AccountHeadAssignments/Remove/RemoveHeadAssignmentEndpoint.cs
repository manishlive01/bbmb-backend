using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Remove
{
    public sealed class RemoveHeadAssignmentEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/account-head-assignments/{id:long}/remove",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new RemoveHeadAssignmentCommand(id), ct)))
                .RequireAuthorization("BudgetRight:F17.Assign")
                .WithName("RemoveAccountHeadAssignment")
                .WithTags("AccountHeadAssignments")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
