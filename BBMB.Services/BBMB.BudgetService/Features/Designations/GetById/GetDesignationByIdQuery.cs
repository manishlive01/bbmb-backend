using BBMB.BudgetService.Features.Designations.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public sealed record GetDesignationByIdQuery(GetByStringIdRequestDto Request) : IRequest<GetDesignationByIdResponseDto>;
}
