using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.Create
{
    public class CreateBudgetEntryHandler : IRequestHandler<CreateBudgetEntryCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateBudgetEntryHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateBudgetEntryCommand request, CancellationToken ct)
        {
            // Validate division exists
            var divisionExists = await _context.Divisions
                .AnyAsync(d => d.DivisionCode == request.dto.DivisionCode, ct);
            if (!divisionExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{request.dto.DivisionCode}' not found"
                };
            }

            // Validate work code if provided
            if (!string.IsNullOrWhiteSpace(request.dto.WorkCode.ToString()))
            {
                var workExists = await _context.Works
                    .AnyAsync(w => w.WorkCode == request.dto.WorkCode, ct);
                if (!workExists)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Work with code '{request.dto.WorkCode}' not found"
                    };
                }
            }

            // Validate major head exists
            var majorHeadExists = await _context.MajorHeads
                .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);
            if (!majorHeadExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Major head with code '{request.dto.MajorAccCode}' not found"
                };
            }

            // Map DTO to Entity
            var budgetEntry = request.dto.Adapt<BudgetEntry>();

            // Persist
            _context.BudgetEntries.Add(budgetEntry);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Budget entry created successfully"
            };
        }
    }
}
