using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetList
{
    public sealed class GetAdminApprovalListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: the work filter travels in the body, not the URL (security requirement).
            app.MapPost("/api/admin-approvals/list",
                    async (WorkFilterRequestDto? dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAdminApprovalListQuery(dto?.WorkId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAdminApprovals")
                .WithTags("AdminApprovals")
                .Produces<Response<List<AdminApprovalDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
