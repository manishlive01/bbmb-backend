using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public sealed record UpdateHeadOfAccountCommand(int Id, UpdateHeadOfAccountDto dto) : IRequest<Response>;
}
