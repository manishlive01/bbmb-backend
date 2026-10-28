using BBMB.BudgetService.Features.Estimates.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    // P4 (design §4.6): the estimate is addressed by its work id, not the old int work code.
    public record UpdateEstimateCommand(int WorkId, string EstimateCode, UpdateEstimateDto Dto) : IRequest<Response<string>>;
}
