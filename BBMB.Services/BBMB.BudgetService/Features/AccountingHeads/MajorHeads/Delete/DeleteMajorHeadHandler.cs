using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Delete
{
    public class DeleteMajorHeadHandler
        : IRequestHandler<DeleteMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteMajorHeadCommand request,
            CancellationToken ct)
        {
            var majorAccCode = request.Request.Id; // 🔥 from GetByStringIdRequestDto

            // 1️⃣ Fetch Major Head
            var majorHead = await _context.MajorHeads
                .Include(x => x.SubMajorHeads)
                .FirstOrDefaultAsync(
                    x => x.MajorAccCode == majorAccCode,
                    ct);

            if (majorHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Major Head not found"
                };
            }

            // 2️⃣ Dependency check
            if (majorHead.SubMajorHeads.Any())
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete Major Head because Sub Major Heads exist"
                };
            }

            // 3️⃣ Delete
            _context.MajorHeads.Remove(majorHead);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Major Head deleted successfully"
            };
        }
    }
}
