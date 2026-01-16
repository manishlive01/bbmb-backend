using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update
{
    public class UpdateSubMinorHeadHandler
        : IRequestHandler<UpdateSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateSubMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateSubMinorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Fetch existing Sub Minor Head (by composite keys)
            var subMinorHead = await _context.SubMinorHeads
                .FirstOrDefaultAsync(x =>
                    x.MinorAccCode == dto.MinorAccCode &&
                    x.SubMinorAccCode == dto.SubMinorAccCode,
                    ct);

            if (subMinorHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Sub Minor Head not found"
                };
            }

            // 2️⃣ Update allowed fields only
            subMinorHead.MinorAccCode = dto.MinorAccCode;
            subMinorHead.SubMinorAccCode = dto.SubMinorAccCode;
            subMinorHead.SubMinorAccDesc = dto.SubMinorAccDesc;
            subMinorHead.ModificationDate = DateTime.UtcNow;
            subMinorHead.UserCode = " ";

            // 3️⃣ Persist
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Sub Minor Head updated successfully"
            };
        }
    }
}
