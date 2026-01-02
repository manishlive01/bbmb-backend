using BBMB_Budget.Shared.RequestDTOs;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;

namespace BBMB_Budget.Features.Geographical.Districts.Delete
{
    public sealed record DeleteDistrictMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
