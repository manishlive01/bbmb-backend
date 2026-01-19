using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Delete
{
    public sealed record DeleteHeadOfAccountCommand(int Id) : IRequest<Response>;
}
