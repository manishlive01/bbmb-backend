using BBMB.BudgetService.Features.MinorHeads.SubMinor.GetByMinorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.GetByMinorHead
{
    public record GetSubMinorHeadsByMinorHeadQuery(string MinorAccCode) : IRequest<List<GetSubMinorHeadsByMinorHeadResponseDto>>;
}
