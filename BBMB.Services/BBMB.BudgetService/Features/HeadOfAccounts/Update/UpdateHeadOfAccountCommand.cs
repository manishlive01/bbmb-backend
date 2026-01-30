using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public sealed record UpdateHeadOfAccountCommand(
        string MajorAccCode,
        string? SubMajorAccCode,
        string MinorAccCode,
        string? SubMinorAccCode,
        string DetailAccCode,
        string ObjectAccCode,
        UpdateHeadOfAccountDto dto
    ) : IRequest<Response>;
}
