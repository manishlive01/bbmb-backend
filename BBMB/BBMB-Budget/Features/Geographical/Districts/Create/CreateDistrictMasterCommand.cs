using BBMB_Budget.Features.Geographical.Districts.Create.Dtos;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.Districts.Create
{
    public sealed record CreateDistrictMasterCommand(CreateDistrictMasterDto dto) : IRequest<Response>;
}
