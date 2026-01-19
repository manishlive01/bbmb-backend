using BBMB.BudgetService.Features.Regions.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.GetById
{
    public sealed record GetRegionByIdQuery(string Code) : IRequest<GetRegionByIdResponseDto>;
}
