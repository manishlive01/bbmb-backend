using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Pools.Delete
{
    public class DeletePoolHandler : IRequestHandler<DeletePoolCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeletePoolHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeletePoolCommand request, CancellationToken ct)
        {
            var pool = await _context.Pools
                .FirstOrDefaultAsync(p => p.PoolCode == request.Code, ct);

            if (pool == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Pool with code '{request.Code}' not found"
                };
            }

            _context.Pools.Remove(pool);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Pool deleted successfully"
            };
        }
    }
}
