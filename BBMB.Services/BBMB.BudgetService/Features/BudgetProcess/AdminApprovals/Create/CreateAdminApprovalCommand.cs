using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Create
{
    public sealed record CreateAdminApprovalCommand(CreateAdminApprovalDto Dto) : IRequest<Response<CreateAdminApprovalResponseDto>>;
}
