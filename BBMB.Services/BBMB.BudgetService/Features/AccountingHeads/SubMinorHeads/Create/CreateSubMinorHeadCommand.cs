using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create
{
    public sealed record CreateSubMinorHeadCommand(CreateSubMinorHeadDto dto)
        : IRequest<Response>;
}
