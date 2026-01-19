using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Delete
{
    public class DeleteHeadOfAccountHandler : IRequestHandler<DeleteHeadOfAccountCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteHeadOfAccountHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteHeadOfAccountCommand request, CancellationToken ct)
        {
            var headOfAccount = await _context.HeadOfAccounts
                .FirstOrDefaultAsync(h => h.Id == request.Id, ct);

            if (headOfAccount == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Head of account with ID '{request.Id}' not found"
                };

            _context.HeadOfAccounts.Remove(headOfAccount);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete head of account. It has associated records."
                };
            }

            return new Response
            {
                IsSucced = true,
                Message = "Head of account deleted successfully"
            };
        }
    }
}
