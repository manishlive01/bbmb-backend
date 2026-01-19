using BBMB.BudgetService.Features.Divisions.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.GetById
{
    public sealed record GetDivisionByIdQuery(int Code) : IRequest<GetDivisionByIdResponseDto>;
}
