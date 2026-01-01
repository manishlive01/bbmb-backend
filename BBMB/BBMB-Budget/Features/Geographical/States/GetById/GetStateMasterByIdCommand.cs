using BBMB_Budget.Features.Geographical.States.GetById.Dtos;
using BBMB_Budget.Shared.RequestDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.States.GetById
{
    public sealed record GetStateMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetStateMasterByIdResponseDto?>;
}
