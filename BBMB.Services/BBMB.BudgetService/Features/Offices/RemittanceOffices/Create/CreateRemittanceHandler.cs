using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.RemittanceOffices.Create
{
    public class CreateRemittanceHandler : IRequestHandler<CreateRemittanceCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateRemittanceHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateRemittanceCommand request, CancellationToken ct)
        {
            var exists = await _context.RemittanceOffices.AsNoTracking()
                .AnyAsync(p => p.RoCode == request.dto.RoCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Remittance with code '{request.dto.RoCode}' already exists"
                };
            }

            var remittance = request.dto.Adapt<RemittanceOffice>();

            await _context.RemittanceOffices.AddAsync(remittance);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Remittance created successfully"
            };
        }
    }
}
