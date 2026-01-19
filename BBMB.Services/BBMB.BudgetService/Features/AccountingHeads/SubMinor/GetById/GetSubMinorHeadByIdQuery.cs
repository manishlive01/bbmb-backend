using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetById
{
    public record GetSubMinorHeadByIdQuery(string SubMinorAccCode) : IRequest<GetSubMinorHeadByIdResponseDto>;
}
