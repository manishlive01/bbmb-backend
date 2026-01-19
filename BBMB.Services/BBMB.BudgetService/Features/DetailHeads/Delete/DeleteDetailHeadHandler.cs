using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.Delete
{
    public class DeleteDetailHeadHandler : IRequestHandler<DeleteDetailHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteDetailHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteDetailHeadCommand request, CancellationToken cancellationToken)
        {
            var detailHead = await _context.DetailHeads
                .FirstOrDefaultAsync(d => d.DetailAccCode == request.DetailAccCode, cancellationToken);

            if (detailHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Detail Head with code '{request.DetailAccCode}' not found."
                };
            }

            try
            {
                _context.DetailHeads.Remove(detailHead);
                await _context.SaveChangesAsync(cancellationToken);

                return new Response
                {
                    IsSucced = true,
                    Message = $"Detail Head '{request.DetailAccCode}' deleted successfully."
                };
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Cannot delete Detail Head '{request.DetailAccCode}' because it has dependencies."
                };
            }
        }
    }
}
