using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign
{
    public sealed class AssignHeadsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/account-head-assignments",
                    async (AssignHeadsDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new AssignHeadsCommand(dto), ct)))
                .RequireAuthorization("BudgetRight:F17.Assign")
                .WithName("AssignAccountHeads")
                .WithTags("AccountHeadAssignments")
                .Produces<Response<int>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
