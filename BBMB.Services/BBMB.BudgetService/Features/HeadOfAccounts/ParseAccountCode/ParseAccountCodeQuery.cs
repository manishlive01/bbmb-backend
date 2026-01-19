using BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode
{
    public sealed record ParseAccountCodeQuery(string AccountCode) : IRequest<ParseAccountCodeResponseDto>;
}
