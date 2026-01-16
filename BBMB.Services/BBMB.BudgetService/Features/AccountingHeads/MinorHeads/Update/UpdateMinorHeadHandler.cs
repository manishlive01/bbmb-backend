using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public class UpdateMinorHeadHandler
        : IRequestHandler<UpdateMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateMinorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

           
            // 2️⃣ Fetch existing Minor Head (current record)
            var minorHead = await _context.MinorHeads
                .FirstOrDefaultAsync(x => x.MinorAccCode == dto.MinorAccCode, ct);

            if (minorHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Minor Head not found"
                };
            }

            // 3️⃣ Duplicate check (exclude current record)
            var duplicateExists = await _context.MinorHeads
                .AnyAsync(x =>
                    x.MinorAccCode != dto.MinorAccCode,
                    ct);

            if (duplicateExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Minor Account Code already exists"
                };
            }

            // 4️⃣ Update ALL allowed fields (including code)
            minorHead.MinorAccCode = dto.MinorAccCode;
            minorHead.MinorAccDesc = dto.MinorAccDesc.Trim();
            minorHead.UserCode = " ";
            minorHead.ModificationDate = DateTime.UtcNow;

            // 5️⃣ Persist
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Minor Head updated successfully"
            };
        }
    }
}
