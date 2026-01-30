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
            try
            {
                var affectedRows = await _context.HeadOfAccounts
                    .Where(h =>
                        h.MajorAccCode == request.MajorAccCode &&
                        h.SubMajorAccCode == request.SubMajorAccCode &&
                        h.MinorAccCode == request.MinorAccCode &&
                        h.SubMinorAccCode == request.SubMinorAccCode &&
                        h.DetailAccCode == request.DetailAccCode &&
                        h.ObjectAccCode == request.ObjectAccCode)
                    .ExecuteDeleteAsync(ct);

                if (affectedRows == 0)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Head of account not found"
                    };
                }

                return new Response
                {
                    IsSucced = true,
                    Message = "Head of account deleted successfully"
                };
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete head of account. It is referenced by other records."
                };
            }
        }


    }
}
