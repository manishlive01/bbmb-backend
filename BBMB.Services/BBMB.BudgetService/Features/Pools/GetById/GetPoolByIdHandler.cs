using BBMB.BudgetService.Features.Pools.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Pools.GetById
{
    public class GetPoolByIdHandler : IRequestHandler<GetPoolByIdQuery, GetPoolByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetPoolByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetPoolByIdResponseDto> Handle(GetPoolByIdQuery request, CancellationToken ct)
        {
            var pool = await _context.Pools
                .FirstOrDefaultAsync(p => p.PoolCode == request.Code, ct);

            if (pool == null)
            {
                throw new KeyNotFoundException($"Pool with code '{request.Code}' not found");
            }

            return new GetPoolByIdResponseDto
            {
                PoolCode = pool.PoolCode,
                PoolDesc = pool.PoolDesc,
                ModificationDate = pool.ModificationDate,
                UserCode = pool.UserCode
            };
        }
    }
}
