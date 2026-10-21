using BBMB.BudgetService.Features.BudgetProcess.Settings.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.Create
{
    public sealed record CreateBudgetSettingCommand(CreateBudgetSettingDto Dto) : IRequest<Response<long>>;
}
