using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Delete
{
    public class DeleteCRSectionManagementHandler
        : IRequestHandler<DeleteCRSectionManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteCRSectionManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteCRSectionManagementCommand request, CancellationToken ct)
        {
            var entity = await _context.Set<CRSection>()
                .FirstOrDefaultAsync(x => x.Id == request.crSectionId, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CR Section record not found"
                };
            }


            _context.Set<CRSection>().Remove(entity);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Section record deleted successfully"
            };
        }
    }
}