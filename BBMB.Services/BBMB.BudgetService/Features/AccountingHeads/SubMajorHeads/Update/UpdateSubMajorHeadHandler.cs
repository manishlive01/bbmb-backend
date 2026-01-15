using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadHandler
        : IRequestHandler<UpdateSubMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateSubMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateSubMajorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 2️⃣ Check Major Head exists
            var majorExists = await _context.MajorHeads
                .AnyAsync(x => x.MajorAccCode == dto.MajorAccCode, ct);

            if (!majorExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Major Head does not exist"
                };
            }

            // 3️⃣ Fetch existing Sub Major Head (current record)
            var subMajorHead = await _context.SubMajorHeads
                .FirstOrDefaultAsync(x =>
                    x.MajorAccCode == dto.MajorAccCode &&
                    x.SubMajorAccCode == dto.SubMajorAccCode,
                    ct);

            if (subMajorHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Sub Major Head not found"
                };
            }

            // 4️⃣ Duplicate check (exclude current record)
            var duplicateExists = await _context.SubMajorHeads
                .AnyAsync(x =>
                    x.MajorAccCode == dto.MajorAccCode &&
                    x.SubMajorAccCode == dto.SubMajorAccCode &&
                    x.Id != subMajorHead.Id,
                    ct);

            if (duplicateExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Sub Major Account Code already exists"
                };
            }

            // 5️⃣ Update ALL fields
            subMajorHead.MajorAccCode = dto.MajorAccCode;
            subMajorHead.SubMajorAccCode = dto.SubMajorAccCode;
            subMajorHead.SubMajorAccDesc = dto.SubMajorAccDesc;
            subMajorHead.ModificationDate = DateTime.UtcNow;

            // 6️⃣ Persist
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Sub Major Head updated successfully"
            };
        }
    }
}
