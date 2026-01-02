using BBMB_Budget.Features.Geographical.Districts.GetById.Dtos;
using BBMB_Budget.Shared.RequestDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.Districts.GetById
{
    public sealed record GetDistrictMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetDistrictMasterByIdResponseDto?>;

}
