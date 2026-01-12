using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.Delete
{
    public sealed record DeleteDistrictMasterCommand(
        GetByIdRequestDto Request)
        : IRequest<Response>;
}
