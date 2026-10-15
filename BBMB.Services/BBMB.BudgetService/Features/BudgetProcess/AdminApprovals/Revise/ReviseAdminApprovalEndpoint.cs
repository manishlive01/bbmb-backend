using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise
{
    public sealed class ReviseAdminApprovalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/admin-approvals/{id:long}/revisions",
                    async (long id, ReviseAdminApprovalDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new ReviseAdminApprovalCommand(id, dto), ct),
                            r => Results.Created($"/api/admin-approvals/{id}", r)))
                .RequireAuthorization("BudgetRight:F03.Enter")
                .WithName("ReviseAdminApproval")
                .WithTags("AdminApprovals")
                .Produces<Response<int>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
