using BBMB.BudgetService.Features.Designations.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public sealed record GetDesignationByIdQuery(string Code) : IRequest<GetDesignationByIdResponseDto>;
}
