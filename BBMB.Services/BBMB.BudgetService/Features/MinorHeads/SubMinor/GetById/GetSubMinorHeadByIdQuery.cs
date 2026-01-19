using BBMB.BudgetService.Features.MinorHeads.SubMinor.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.GetById
{
    public record GetSubMinorHeadByIdQuery(string SubMinorAccCode) : IRequest<GetSubMinorHeadByIdResponseDto>;
}
