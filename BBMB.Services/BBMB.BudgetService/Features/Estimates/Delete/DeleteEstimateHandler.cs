using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Delete
{
    public class DeleteEstimateHandler : IRequestHandler<DeleteEstimateCommand, Response>
    {
        public Task<Response> Handle(DeleteEstimateCommand request, CancellationToken ct)
        {
            // P4 (design §4.6, §9.3 Estimates P4): the legacy estimate delete route refuses after the reset (M-19).
            // T-23/T-07 carry no DELETE path for estimate records once the work process owns them (DB-13).
            return Task.FromResult(BudgetResponses.Fail(BudgetMessages.M19));
        }
    }
}
