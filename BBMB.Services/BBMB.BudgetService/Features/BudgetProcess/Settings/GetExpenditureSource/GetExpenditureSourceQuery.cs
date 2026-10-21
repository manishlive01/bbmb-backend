using BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Settings.GetExpenditureSource
{
    public sealed record GetExpenditureSourceQuery(long OfficeId, int FinYear) : IRequest<Response<ExpenditureSourceDto>>;
}
