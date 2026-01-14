using BBMB.BudgetService.Features.FinancialYears.Create.Dtos;
using BBMB.BudgetService.Features.Geographical.States.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public sealed record CreateFinancialYearCommand(CreateFinancialYearDto dto) : IRequest<Response>;
}
