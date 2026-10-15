using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
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
            // D-2 (design §3): the DTO carries the Division code, the entity the Division id. Resolve the
            // id here; Adapt alone left DivisionId = 0, which fails the FK or links the wrong row.
            var divisionId = await _context.Divisions
                .Where(d => d.DivisionCode == request.dto.DivisionCode)
                .Select(d => (int?)d.Id)
                .FirstOrDefaultAsync(ct);
            if (divisionId is null)
            {
                return BudgetResponses.Fail(BudgetMessages.M05);
            }

            // Validate work code if provided. P4 compile fix (design §4.6): work_code is now string?; the DTO
            // is re-keyed to WorkId in P4-03, so the lookup compares the string form of the code until then.
            var workCodeText = request.dto.WorkCode.ToString();
            if (!string.IsNullOrWhiteSpace(workCodeText))
            {
                var workExists = await _context.Works
                    .AnyAsync(w => w.WorkCode == workCodeText, ct);
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
            budgetEntry.DivisionId = divisionId.Value;

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
