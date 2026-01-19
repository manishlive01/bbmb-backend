using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Delete
{
    public class DeleteSubMajorHeadHandler : IRequestHandler<DeleteSubMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        
        public DeleteSubMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }
        
        public async Task<Response> Handle(DeleteSubMajorHeadCommand request, CancellationToken ct)
        {
            var subMajorHead = await _context.SubMajorHeads
                .FirstOrDefaultAsync(s => s.SubMajorAccCode == request.SubMajorAccCode, ct);
                
            if (subMajorHead == null)
                return new Response 
                { 
                    IsSucced = false, 
                    Message = $"Sub-major head with code '{request.SubMajorAccCode}' not found" 
                };
            
            _context.SubMajorHeads.Remove(subMajorHead);
            
            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response 
                { 
                    IsSucced = false, 
                    Message = "Cannot delete sub-major head. It has associated records." 
                };
            }
            
            return new Response 
            { 
                IsSucced = true, 
                Message = "Sub-major head deleted successfully" 
            };
        }
    }
}
