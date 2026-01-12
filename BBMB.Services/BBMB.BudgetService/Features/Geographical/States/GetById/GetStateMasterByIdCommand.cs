using BBMB.BudgetService.Features.Geographical.States.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.GetById
{
    public sealed record GetStateMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetStateMasterByIdResponseDto?>;
}
