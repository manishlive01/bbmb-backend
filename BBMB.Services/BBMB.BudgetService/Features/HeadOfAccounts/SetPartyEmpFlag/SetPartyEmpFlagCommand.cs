using BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag
{
    public sealed record SetPartyEmpFlagCommand(long AccountCodeId, SetPartyEmpFlagDto Dto) : IRequest<Response>;
}
