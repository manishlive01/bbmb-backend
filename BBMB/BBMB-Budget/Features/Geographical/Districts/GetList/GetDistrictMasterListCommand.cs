using BBMB_Budget.Features.Geographical.Districts.GetList.Dtos;
using MediatR;

namespace BBMB_Budget.Features.Geographical.Districts.GetList
{
    public sealed record GetDistrictMasterListCommand() : IRequest<List<GetDistrictMasterListResponseDto>>;
}
