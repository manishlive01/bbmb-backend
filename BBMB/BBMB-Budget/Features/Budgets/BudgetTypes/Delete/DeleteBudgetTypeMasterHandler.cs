using BBMB_Budget.Infrastructure.Persistence;
using BBMB_Budget.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Delete
{
    public class DeleteBudgetTypeMasterHandler
        : IRequestHandler<DeleteBudgetTypeMasterCommand, Response>
    {
        private readonly BBMBContext _context;

        public DeleteBudgetTypeMasterHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteBudgetTypeMasterCommand request,
            CancellationToken ct)
        {
            var id = request.Request.Id;

            // 1️⃣ Fetch entity
            var budgetType = await _context.BudgetTypeMasters
                .FirstOrDefaultAsync(x =>
                    x.BudgetTypeMasterId == id &&
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

            // 2️⃣ Soft delete
            budgetType.IsDeleted = true;
            budgetType.DeletedAt = DateTime.UtcNow;

            // Optional: deactivate
            budgetType.IsActive = false;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Budget type deleted successfully"
            };
        }
    }
}
