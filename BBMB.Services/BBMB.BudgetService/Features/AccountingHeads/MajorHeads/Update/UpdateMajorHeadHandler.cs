using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public class UpdateMajorHeadHandler
        : IRequestHandler<UpdateMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateMajorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Fetch existing entity
            var majorHead = await _context.MajorHeads
                .FirstOrDefaultAsync(x => x.MajorAccCode == dto.MajorAccCode, ct);

            if (majorHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Major Head not found"
                };
            }

            // 2️⃣ Duplicate check (exclude current record)
            var duplicateExists = await _context.MajorHeads
                .AnyAsync(x =>
                    x.MajorAccCode != majorHead.MajorAccCode,
                    ct);

            if (duplicateExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Major Account Code already exists"
                };
            }

            majorHead.MajorAccCode = dto.MajorAccCode;
            majorHead.UserCode = " ";
            majorHead.ModificationDate = DateTime.UtcNow;

            // 4️⃣ Persist
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Major Head updated successfully"
            };
        }
    }
}
