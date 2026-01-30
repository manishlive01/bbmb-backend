using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
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
                .FirstOrDefaultAsync(h =>
                    h.MajorAccCode == request.MajorAccCode &&
                    h.SubMajorAccCode == request.SubMajorAccCode &&
                    h.MinorAccCode == request.MinorAccCode &&
                    h.SubMinorAccCode == request.SubMinorAccCode &&
                    h.DetailAccCode == request.DetailAccCode &&
                    h.ObjectAccCode == request.ObjectAccCode,
                    ct);

            if (headOfAccount == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Head of account not found"
                };
            }

            // 🔹 Update ONLY allowed fields (NO FK updates)
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
