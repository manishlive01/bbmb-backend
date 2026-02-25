using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CEManagements.Create
{
    public class CreateCEManagementHandler : IRequestHandler<CreateCEManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateCEManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateCEManagementCommand request, CancellationToken ct)
        {
            // ✅ Map DTO to Entity
            var ce = request.dto.Adapt<CEManagement>();

            // 🔎 Optional: Check duplicate (if needed)
            var exists = await _context.CEManagements
                .AnyAsync(x => x.CECode == request.dto.CECode, ct);
            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CE Code already exists with this name."
                };
            }

            // ✅ Persist
            _context.CEManagements.Add(ce);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CE record created successfully"
            };
        }
    }
}
