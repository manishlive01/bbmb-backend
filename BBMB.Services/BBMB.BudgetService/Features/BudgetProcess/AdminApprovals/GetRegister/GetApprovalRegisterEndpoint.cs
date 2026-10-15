using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister
{
    public sealed class GetApprovalRegisterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin-approvals/register",
                    async (string? level, long? officeId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetApprovalRegisterQuery(level, officeId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetApprovalRegister")
                .WithTags("BudgetApprovals")
                .Produces<Response<List<ApprovalRegisterRowDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
