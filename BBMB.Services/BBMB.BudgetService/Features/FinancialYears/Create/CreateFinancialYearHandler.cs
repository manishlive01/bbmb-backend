using BBMB.BudgetService.Features.FinancialYears.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.FinancialYears.Create
{
    public class CreateFinancialYearHandler
        : IRequestHandler<CreateFinancialYearCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateFinancialYearHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateFinancialYearCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Map DTO → Entity
            var financialYear = request.dto.Adapt<FinancialYear>();

            // 2️⃣ Duplicate check (FinYearClosing)
            var exists = await _context.FinancialYears
                .AnyAsync(x =>
                    x.FinYearClosing == financialYear.FinYearClosing,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Financial year already exists"
                };
            }

            // 4️⃣ Persist
            _context.FinancialYears.Add(financialYear);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Financial year created successfully"
            };
        }
    }
}
