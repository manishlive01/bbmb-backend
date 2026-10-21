using BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.SetExpenditureSource
{
    public sealed record SetExpenditureSourceCommand(SetExpenditureSourceDto Dto) : IRequest<Response>;
}
