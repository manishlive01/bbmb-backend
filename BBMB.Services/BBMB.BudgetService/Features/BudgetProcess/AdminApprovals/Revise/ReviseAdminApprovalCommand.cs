using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Revise
{
    /// <summary>Returns the new version number.</summary>
    public sealed record ReviseAdminApprovalCommand(long AdminApprovalId, ReviseAdminApprovalDto Dto) : IRequest<Response<int>>;
}
