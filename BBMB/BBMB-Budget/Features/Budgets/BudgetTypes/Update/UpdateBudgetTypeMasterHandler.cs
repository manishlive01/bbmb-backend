using BBMB_Budget.Infrastructure.Persistence;
using BBMB_Budget.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Update
{
    public class UpdateBudgetTypeMasterHandler
        : IRequestHandler<UpdateBudgetTypeMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public UpdateBudgetTypeMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateBudgetTypeMasterCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            // 1️⃣ Fetch existing Budget Type
            var budgetType = await _context.BudgetTypeMasters
                .FirstOrDefaultAsync(x =>
                    x.BudgetTypeMasterId == dto.BudgetTypeMasterId &&
                    x.IsDeleted == false,
                    ct);

            if (budgetType == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Budget type not found"
                };
            }

            // 2️⃣ Check duplicate BudgetTypeCode (excluding self)
            var duplicate = await _context.BudgetTypeMasters
                .AnyAsync(x =>
                    x.BudgetTypeCode == dto.BudgetTypeCode &&
                    x.BudgetTypeMasterId != dto.BudgetTypeMasterId &&
                    x.IsDeleted == false,
                    ct);

            if (duplicate)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Budget type code already exists"
                };
            }

            // 3️⃣ Map updated fields
            dto.Adapt(budgetType);

            // 4️⃣ System fields
            budgetType.UpdatedAt = DateTime.UtcNow;

            // 5️⃣ Save
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Budget type updated successfully"
            };
        }
    }
}
