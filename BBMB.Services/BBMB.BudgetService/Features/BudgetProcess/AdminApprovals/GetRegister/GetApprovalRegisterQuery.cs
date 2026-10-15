using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetRegister
{
    /// <summary>
    /// Design §8.8 (<c>GET /api/admin-approvals/register?level=&amp;officeId=</c>, F-05, BUD-066, BUD-067): the
    /// administrative approvals of the offices in scope, each with all its versions.
    /// </summary>
    public sealed record GetApprovalRegisterQuery(string? Level, long? OfficeId)
        : IRequest<Response<List<ApprovalRegisterRowDto>>>;
}
