using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Update
{
    public class UpdateCRSectionManagementHandler
        : IRequestHandler<UpdateCRSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateCRSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateCRSectionManagementCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            // 🔎 Check if record exists
            var entity = await _context.Set<CRSection>()
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CR Section not found"
                };
            }

            // 🔥 Validate CRId (FK)
            var crExists = await _context.CRManagements.AsNoTracking()
                .AnyAsync(x => x.Id == dto.CRId, ct);

            if (!crExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Invalid CRId"
                };
            }

            // 🔎 Duplicate check (Name + CRId)
            var exists = await _context.Set<CRSection>()
                .AnyAsync(x => x.Id != dto.Id &&
                               x.Name == dto.Name &&
                               x.CRId == dto.CRId, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CR Section already exists with this name"
                };
            }

            // ✅ Update using Mapster
            dto.Adapt(entity);

            entity.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Section updated successfully"
            };
        }
    }
}