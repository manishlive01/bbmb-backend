using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update
{
    public sealed record UpdateSubMinorHeadCommand(UpdateSubMinorHeadDto dto)
        : IRequest<Response>;
}
