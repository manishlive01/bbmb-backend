using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Update
{
    public class UpdateEstimateHandler : IRequestHandler<UpdateEstimateCommand, Response<string>>
    {
        public Task<Response<string>> Handle(UpdateEstimateCommand request, CancellationToken cancellationToken)
        {
            // P4 (design §4.6, §9.3 Estimates P4): the legacy estimate write route refuses after the reset (M-19).
            // Estimate figures are maintained by Financial Accounting against a technical sanction, not here.
            return Task.FromResult(BudgetResponses.Fail<string>(BudgetMessages.M19));
        }
    }
}
