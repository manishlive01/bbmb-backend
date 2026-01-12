using BBMB.BudgetService.Features.Geographical.Districts.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.GetById
{
    public sealed record GetDistrictMasterByIdCommand(GetByIdRequestDto Request) : IRequest<GetDistrictMasterByIdResponseDto?>;

}
