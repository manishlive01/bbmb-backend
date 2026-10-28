using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    // P4 (design §4.6): the estimate is addressed by its work id, not the old int work code.
    public sealed record DeleteEstimateCommand(int WorkId, string EstimateCode) : IRequest<Response>;
}
