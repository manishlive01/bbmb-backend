using BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.GetByMinorHead
{
    public record GetSubMinorHeadsByMinorHeadQuery(string MinorAccCode) : IRequest<List<GetSubMinorHeadsByMinorHeadResponseDto>>;
}
