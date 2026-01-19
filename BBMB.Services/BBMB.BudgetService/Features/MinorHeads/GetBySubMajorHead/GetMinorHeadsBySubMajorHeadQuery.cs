using BBMB.BudgetService.Features.MinorHeads.GetBySubMajorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.GetBySubMajorHead
{
    public sealed record GetMinorHeadsBySubMajorHeadQuery(string SubMajorAccCode) : IRequest<List<GetMinorHeadsBySubMajorHeadResponseDto>>;
}
