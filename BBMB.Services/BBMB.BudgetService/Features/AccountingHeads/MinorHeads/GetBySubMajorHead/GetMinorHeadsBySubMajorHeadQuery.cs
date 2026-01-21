using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetBySubMajorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetBySubMajorHead
{
    public sealed record GetMinorHeadsBySubMajorHeadQuery(string SubMajorAccCode) : IRequest<List<GetMinorHeadsBySubMajorHeadResponseDto>>;
}
