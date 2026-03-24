using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Create
{
    public class CreateCRSectionManagementHandler : IRequestHandler<CreateCRSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateCRSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateCRSectionManagementCommand request, CancellationToken ct)
        {
            // ✅ Map DTO → Entity
            var entity = request.dto.Adapt<CRSection>();

            // 🔥 FK Validation (CR must exist)
            var crExists = await _context.CRManagements
                .AnyAsync(x => x.Id == request.dto.CRId, ct);

            if (!crExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Invalid CRId. CR does not exist."
                };
            }

            // 🔎 Optional duplicate check (Name per CR)
            var exists = await _context.Set<CRSection>()
                .AnyAsync(x => x.Name == request.dto.Name && x.CRId == request.dto.CRId, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CR Section already exists with this name."
                };
            }

            // ✅ Default values
            entity.Date = DateTime.UtcNow;
            entity.Status = true;

            // ✅ Save
            await _context.Set<CRSection>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Section created successfully"
            };
        }
    }
}