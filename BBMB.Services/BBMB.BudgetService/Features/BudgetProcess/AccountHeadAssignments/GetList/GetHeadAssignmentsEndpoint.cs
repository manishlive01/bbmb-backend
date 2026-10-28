using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList
{
    public sealed class GetHeadAssignmentsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: the office filter travels in the body, not the URL (security requirement).
            app.MapPost("/api/account-head-assignments/list",
                    async (HeadAssignmentListRequestDto? dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetHeadAssignmentsQuery(dto?.OfficeId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAccountHeadAssignments")
                .WithTags("AccountHeadAssignments")
                .Produces<Response<List<AssignedHeadDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
