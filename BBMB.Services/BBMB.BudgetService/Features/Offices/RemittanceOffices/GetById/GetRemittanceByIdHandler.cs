using BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById.Dtos;
using BBMB.BudgetService.Features.Pools.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.GetById
{
    public class GetRemittanceByIdHandler : IRequestHandler<GetRemittanceByIdQuery, GetRemittanceByIdDto>
    {
        private readonly BudgetContext _context;

        public GetRemittanceByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetRemittanceByIdDto> Handle(GetRemittanceByIdQuery request, CancellationToken ct)
        {
            var remittance = await _context.RemittanceOffices.AsNoTracking().FirstOrDefaultAsync(d=>d.RoCode==request.Code);

            if (remittance == null)
            {
                throw new KeyNotFoundException($"Pool with code '{request.Code}' not found");
            }

            return new GetRemittanceByIdDto
            {
                RemittanceCode = remittance.RoCode,
                RemittanceDesc = remittance.RoDesc,
                ModificationDate = remittance.ModificationDate,
               
            };
        }
    }
}
