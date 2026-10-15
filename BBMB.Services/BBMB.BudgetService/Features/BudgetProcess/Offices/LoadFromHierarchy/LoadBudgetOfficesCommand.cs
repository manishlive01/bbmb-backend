using BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy
{
    public sealed record LoadBudgetOfficesCommand(LoadBudgetOfficesDto Dto) : IRequest<Response<LoadBudgetOfficesResponseDto>>;
}
