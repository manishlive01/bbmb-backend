using BBMB.BudgetService.Features.Geographical.Districts.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.Create
{
    public sealed record CreateDistrictMasterCommand(CreateDistrictMasterDto dto) : IRequest<Response>;
}
