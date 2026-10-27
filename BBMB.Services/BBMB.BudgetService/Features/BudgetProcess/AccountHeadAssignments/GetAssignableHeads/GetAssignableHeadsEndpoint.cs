using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetAssignableHeads
{
    public sealed class GetAssignableHeadsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId travels in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/assignable-heads",
                    async (ProposalReadRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAssignableHeadsQuery(dto.ProposalId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAssignableHeads")
                .WithTags("AccountHeadAssignments")
                .Produces<Response<List<AssignedHeadDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
