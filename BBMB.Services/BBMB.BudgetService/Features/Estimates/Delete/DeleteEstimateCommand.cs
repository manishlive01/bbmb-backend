using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    public sealed record DeleteEstimateCommand(string WorkCode, string EstimateCode) : IRequest<Response>;
}
