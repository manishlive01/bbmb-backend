using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Pools.Create
{
    public class CreatePoolHandler : IRequestHandler<CreatePoolCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreatePoolHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreatePoolCommand request, CancellationToken ct)
        {
            var exists = await _context.Pools
                .AnyAsync(p => p.PoolCode == request.dto.PoolCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Pool with code '{request.dto.PoolCode}' already exists"
                };
            }

            var pool = request.dto.Adapt<Pool>();
            pool.ModificationDate = DateTime.UtcNow;

            _context.Pools.Add(pool);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Pool created successfully"
            };
        }
    }
}
