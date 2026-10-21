using BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse
{
    public sealed record MarkNotForUseCommand(long AccountCodeId, MarkNotForUseDto Dto) : IRequest<Response>;
}
