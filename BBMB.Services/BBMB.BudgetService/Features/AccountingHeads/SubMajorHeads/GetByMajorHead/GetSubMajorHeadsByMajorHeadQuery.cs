using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetByMajorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetByMajorHead
{
    public sealed record GetSubMajorHeadsByMajorHeadQuery(string MajorAccCode) : IRequest<List<GetSubMajorHeadsByMajorHeadResponseDto>>;
}
