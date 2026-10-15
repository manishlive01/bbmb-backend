using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    public class CreateEstimateHandler : IRequestHandler<CreateEstimateCommand, Response>
    {
        public Task<Response> Handle(CreateEstimateCommand request, CancellationToken ct)
        {
            // P4 (design §4.6, §9.3 Estimates P4): after the work-code reset the estimate is re-keyed on
            // (work_id, estimate_code) and an estimate code is recorded only by Financial Accounting against a
            // technical sanction (DB-04/DB-05). This legacy write route no longer creates estimates; it refuses
            // with M-19. The GET routes are kept so existing estimate data stays readable.
            return Task.FromResult(BudgetResponses.Fail(BudgetMessages.M19));
        }
    }
}
