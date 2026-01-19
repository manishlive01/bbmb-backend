using BBMB.BudgetService.Features.MinorHeads.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.GetById
{
    public sealed record GetMinorHeadByIdQuery(string MinorAccCode) : IRequest<GetMinorHeadByIdResponseDto>;
}
