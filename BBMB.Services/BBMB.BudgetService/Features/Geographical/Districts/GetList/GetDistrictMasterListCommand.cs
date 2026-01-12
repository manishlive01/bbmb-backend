using BBMB.BudgetService.Features.Geographical.Districts.GetList.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.GetList
{
    public sealed record GetDistrictMasterListCommand() : IRequest<List<GetDistrictMasterListResponseDto>>;
}
