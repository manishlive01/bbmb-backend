using BBMB.BudgetService.Features.DetailHeads.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetById
{
    public sealed record GetDetailHeadByIdQuery(string DetailAccCode) : IRequest<GetDetailHeadByIdResponseDto>;
}
