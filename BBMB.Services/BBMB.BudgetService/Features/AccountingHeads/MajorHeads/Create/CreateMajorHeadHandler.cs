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
            // 1️⃣ Map DTO → Entity
            var majorHead = request.dto.Adapt<MajorHead>();

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
