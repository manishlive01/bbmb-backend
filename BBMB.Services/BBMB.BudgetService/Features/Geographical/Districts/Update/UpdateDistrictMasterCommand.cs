using BBMB.BudgetService.Features.Geographical.Districts.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.Update
{
    public sealed record UpdateDistrictMasterCommand(UpdateDistrictMasterDto dto) : IRequest<Response>;
}
