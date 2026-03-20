using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public record UpdateEstimateCommand(int WorkCode, string EstimateCode, UpdateEstimateDto Dto) : IRequest<Response<string>>;
}
