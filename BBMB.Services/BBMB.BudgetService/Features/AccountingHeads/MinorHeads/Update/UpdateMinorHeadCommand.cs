using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public sealed record UpdateMinorHeadCommand(UpdateMinorHeadDto dto)
        : IRequest<Response>;
}
