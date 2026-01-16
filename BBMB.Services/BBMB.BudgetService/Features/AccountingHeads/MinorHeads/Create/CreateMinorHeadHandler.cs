using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadHandler
        : IRequestHandler<CreateMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateMinorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Duplicate check
            var exists = await _context.MinorHeads
                .AnyAsync(x => x.MinorAccCode == dto.MinorAccCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Minor Account Code already exists"
                };
            }

            // 2️⃣ Map DTO → Entity
            var minorHead = dto.Adapt<MinorHead>();

            // 3️⃣ System fields
            minorHead.ModificationDate = DateTime.UtcNow;
            minorHead.UserCode = " "; // 🔁 replace with logged-in user later

            // 4️⃣ Persist
            _context.MinorHeads.Add(minorHead);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Minor Head created successfully"
            };
        }
    }
}
