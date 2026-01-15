using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadHandler
        : IRequestHandler<CreateSubMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateSubMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateSubMajorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Check Major Head exists
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

            // 2️⃣ Duplicate check (SubMajor under same Major)
            var exists = await _context.SubMajorHeads
                .AnyAsync(x =>
                    x.SubMajorAccCode == dto.SubMajorAccCode &&
                    x.MajorAccCode == dto.MajorAccCode,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Sub Major Head already exists for this Major Head"
                };
            }

            // 3️⃣ Map DTO → Entity
            var subMajorHead = dto.Adapt<SubMajorHead>();

            // 4️⃣ System fields
            subMajorHead.ModificationDate = DateTime.UtcNow;
            subMajorHead.UserCode = " ";

            // 5️⃣ Persist
            _context.SubMajorHeads.Add(subMajorHead);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Sub Major Head created successfully"
            };
        }
    }
}
