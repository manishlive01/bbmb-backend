using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Budgets.Update
{
    public class UpdateBudgetEntryHandler : IRequestHandler<UpdateBudgetEntryCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateBudgetEntryHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateBudgetEntryCommand request, CancellationToken ct)
        {
            var budgetEntry = await _context.BudgetEntries
                .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

            if (budgetEntry == null)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Budget entry with ID '{request.Id}' not found"
                };

            // Validate foreign keys exist
            var divisionExists = await _context.Divisions
                .AnyAsync(d => d.DivisionCode == request.dto.DivisionCode, ct);
            if (!divisionExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Division with code '{request.dto.DivisionCode}' not found"
                };

            // Validate work code if provided
            if (!string.IsNullOrEmpty(request.dto.WorkCode))
            {
                var workExists = await _context.Works
                    .AnyAsync(w => w.WorkCode == request.dto.WorkCode, ct);
                if (!workExists)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Work with code '{request.dto.WorkCode}' not found"
                    };
            }

            // Validate accounting codes exist
            var majorHeadExists = await _context.MajorHeads
                .AnyAsync(m => m.MajorAccCode == request.dto.MajorAccCode, ct);
            if (!majorHeadExists)
                return new Response
                {
                    IsSucced = false,
                    Message = $"Major head with code '{request.dto.MajorAccCode}' not found"
                };

            // Update fields
            budgetEntry.DivisionCode = request.dto.DivisionCode;
            budgetEntry.DivSection = request.dto.DivSection;
            budgetEntry.CalendarYear = request.dto.CalendarYear;
            budgetEntry.SequenceNo = request.dto.SequenceNo;
            budgetEntry.MajorAccCode = request.dto.MajorAccCode;
            budgetEntry.SubMajorAccCode = request.dto.SubMajorAccCode;
            budgetEntry.MinorAccCode = request.dto.MinorAccCode;
            budgetEntry.SubMinorAccCode = request.dto.SubMinorAccCode;
            budgetEntry.DetailAccCode = request.dto.DetailAccCode;
            budgetEntry.ObjectAccCode = request.dto.ObjectAccCode;
            budgetEntry.WorkCode = request.dto.WorkCode;
            budgetEntry.BudgetDescription = request.dto.BudgetDescription;
            budgetEntry.BudgetAmount = request.dto.BudgetAmount;
            budgetEntry.RevisedAmount = request.dto.RevisedAmount;
            budgetEntry.ProposedBudget = request.dto.ProposedBudget;
            budgetEntry.BudgetType = request.dto.BudgetType;
            budgetEntry.UserCode = request.dto.UserCode;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Budget entry updated successfully"
            };
        }
    }
}
