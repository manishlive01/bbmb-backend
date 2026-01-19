using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public class UpdateHeadOfAccountHandler : IRequestHandler<UpdateHeadOfAccountCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateHeadOfAccountHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateHeadOfAccountCommand request, CancellationToken ct)
        {
            var headOfAccount = await _context.HeadOfAccounts
                .FirstOrDefaultAsync(h => h.Id == request.Id, ct);

            if (headOfAccount == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Head of account with ID '{request.Id}' not found"
                };

            // Update only allowed fields (foreign keys cannot be updated)
            headOfAccount.HeadDesc = request.dto.HeadDesc;
            headOfAccount.PartyEmpRequired = request.dto.PartyEmpRequired;
            headOfAccount.PartyLedgToPrint = request.dto.PartyLedgToPrint;
            headOfAccount.TdsDeducted = request.dto.TdsDeducted;
            headOfAccount.AdvanceHead = request.dto.AdvanceHead;
            headOfAccount.BankCash = request.dto.BankCash;
            headOfAccount.MonthlyCarryFwd = request.dto.MonthlyCarryFwd;
            headOfAccount.UserCode = request.dto.UserCode;
            headOfAccount.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Head of account updated successfully"
            };
        }
    }
}
