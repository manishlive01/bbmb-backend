using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Delete
{
    public class DeleteDivSectionManagementHandler
        : IRequestHandler<DeleteDivSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteDivSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteDivSectionManagementCommand request, CancellationToken ct)
        {
            var entity = await _context.Set<DivSection>()
                .FirstOrDefaultAsync(x => x.Id == request.divSectionId, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Division Section not found"
                };
            }

            // 🔥 OPTIONAL: Prevent delete if used somewhere else
            // Example:
            // var isUsed = await _context.SomeOtherTable
            //     .AnyAsync(x => x.DivSectionId == request.divSectionId, ct);
            // if (isUsed)
            // {
            //     return new Response
            //     {
            //         IsSucced = false,
            //         Message = "Cannot delete. This section is in use."
            //     };
            // }

            _context.Set<DivSection>().Remove(entity);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Division Section deleted successfully"
            };
        }
    }
}