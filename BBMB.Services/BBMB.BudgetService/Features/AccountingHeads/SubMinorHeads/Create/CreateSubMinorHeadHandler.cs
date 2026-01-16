using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create
{
    public class CreateSubMinorHeadHandler
        : IRequestHandler<CreateSubMinorHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateSubMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateSubMinorHeadCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Check Minor Head exists
            var minorExists = await _context.MinorHeads
                .AnyAsync(x => x.MinorAccCode == dto.MinorAccCode, ct);

            if (!minorExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Minor Head does not exist"
                };
            }

            // 2️⃣ Duplicate check (SubMinor under same Minor)
            var exists = await _context.SubMinorHeads
                .AnyAsync(x =>
                    x.MinorAccCode == dto.MinorAccCode &&
                    x.SubMinorAccCode == dto.SubMinorAccCode,
                    ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Sub Minor Head already exists for this Minor Head"
                };
            }

            // 3️⃣ Map DTO → Entity
            var subMinorHead = dto.Adapt<SubMinorHead>();

            // 4️⃣ System fields
            subMinorHead.ModificationDate = DateTime.UtcNow;
            subMinorHead.UserCode = " ";

            // 5️⃣ Persist
            _context.SubMinorHeads.Add(subMinorHead);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Sub Minor Head created successfully"
            };
        }
    }
}
