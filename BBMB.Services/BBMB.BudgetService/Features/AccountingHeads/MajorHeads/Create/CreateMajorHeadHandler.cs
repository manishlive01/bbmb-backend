using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public class CreateMajorHeadHandler
        : IRequestHandler<CreateMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateMajorHeadCommand request,
            CancellationToken ct)
        {
            // 1️⃣ Duplicate check
            var exists = await _context.MajorHeads
                .AnyAsync(x =>
                    x.MajorAccCode == request.dto.MajorAccCode,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Major Account Code already exists"
                };
            }

            // 2️⃣ Map DTO → Entity
            var majorHead = request.dto.Adapt<MajorHead>();

            // 3️⃣ System fields
            majorHead.ModificationDate = DateTime.UtcNow;
            majorHead.UserCode = " "; // 🔁 replace with logged-in user later

            // 4️⃣ Persist
            _context.MajorHeads.Add(majorHead);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Major Head created successfully"
            };
        }
    }
}
