using BBMB.BudgetService.Features.DetailHeads.GetBySubMinorHead.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetBySubMinorHead
{
    public sealed record GetDetailHeadsBySubMinorHeadQuery(string SubMinorAccCode) : IRequest<List<GetDetailHeadsBySubMinorHeadResponseDto>>;
}
