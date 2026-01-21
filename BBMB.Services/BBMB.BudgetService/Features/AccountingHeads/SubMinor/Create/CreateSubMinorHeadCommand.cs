using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create
{
    public record CreateSubMinorHeadCommand(CreateSubMinorHeadDto dto) : IRequest<Response>;
}
