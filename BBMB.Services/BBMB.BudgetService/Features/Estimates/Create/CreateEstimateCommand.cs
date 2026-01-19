using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    public sealed record CreateEstimateCommand(CreateEstimateDto dto) : IRequest<Response>;
}
