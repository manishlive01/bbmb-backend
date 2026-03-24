using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Update
{
    public class UpdateDivSectionManagementHandler
        : IRequestHandler<UpdateDivSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDivSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateDivSectionManagementCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            var entity = await _context.Set<DivSection>()
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Division Section not found"
                };
            }

            // 🔥 FK Validation
            var divisionExists = await _context.Divisions
                .AnyAsync(x => x.Id == dto.DivisionId, ct);

            if (!divisionExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Invalid DivisionId"
                };
            }

            // 🔎 Duplicate check
            var exists = await _context.Set<DivSection>()
                .AnyAsync(x =>
                    x.Id != dto.Id &&
                    x.DivisionId == dto.DivisionId &&
                    x.DivSectionCode == dto.DivSectionCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "DivSectionCode already exists for this Division"
                };
            }

            // ✅ Map
            dto.Adapt(entity);
            entity.Date = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Division Section updated successfully"
            };
        }
    }
}