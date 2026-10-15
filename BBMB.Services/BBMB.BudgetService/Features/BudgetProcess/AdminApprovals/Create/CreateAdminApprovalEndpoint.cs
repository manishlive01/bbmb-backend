using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create
{
    public sealed class CreateAdminApprovalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/admin-approvals",
                    async (CreateAdminApprovalDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new CreateAdminApprovalCommand(dto), ct),
                            r => Results.Created($"/api/admin-approvals/{r.Data!.AdminApprovalId}", r)))
                .RequireAuthorization("BudgetRight:F03.Enter")
                .WithName("CreateAdminApproval")
                .WithTags("AdminApprovals")
                .Produces<Response<CreateAdminApprovalResponseDto>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<CreateAdminApprovalResponseDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
