using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public sealed record CreateHeadOfAccountCommand(CreateHeadOfAccountDto dto) : IRequest<Response>;
}
