using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Create
{
    public class CreateDivSectionManagementHandler
        : IRequestHandler<CreateDivSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateDivSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateDivSectionManagementCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            // 🔥 FK Validation (Division)
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
            var entity = dto.Adapt<DivSection>();
            entity.Date = DateTime.UtcNow;

            await _context.Set<DivSection>().AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Division Section created successfully"
            };
        }
    }
}