using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Pools.Update
{
    public class UpdatePoolHandler : IRequestHandler<UpdatePoolCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdatePoolHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdatePoolCommand request, CancellationToken ct)
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

            pool.PoolDesc = request.dto.PoolDesc;
            pool.UserCode = request.dto.UserCode;
            pool.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Pool updated successfully"
            };
        }
    }
}
