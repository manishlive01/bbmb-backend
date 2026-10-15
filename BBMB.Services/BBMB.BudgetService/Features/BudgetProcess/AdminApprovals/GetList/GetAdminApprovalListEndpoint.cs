using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetList
{
    public sealed class GetAdminApprovalListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin-approvals",
                    async (IMediator mediator, CancellationToken ct, [FromQuery] int? workId = null) =>
                        BudgetResults.Ok(await mediator.Send(new GetAdminApprovalListQuery(workId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAdminApprovals")
                .WithTags("AdminApprovals")
                .Produces<Response<List<AdminApprovalDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
