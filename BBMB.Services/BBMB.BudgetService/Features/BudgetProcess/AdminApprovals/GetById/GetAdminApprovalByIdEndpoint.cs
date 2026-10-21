using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetById
{
    public sealed class GetAdminApprovalByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin-approvals/{id:long}",
                    async (long id, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAdminApprovalByIdQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAdminApprovalById")
                .WithTags("AdminApprovals")
                .Produces<Response<AdminApprovalDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
