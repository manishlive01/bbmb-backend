using BBMB_Budget.Features.Geographical.Districts.Update.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.Districts.Update
{
    public sealed record UpdateDistrictMasterCommand(UpdateDistrictMasterDto dto) : IRequest<Response>;
}
