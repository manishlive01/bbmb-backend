using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadHandler : IRequestHandler<UpdateSubMajorHeadCommand, Response>
    {
        private readonly BudgetContext _context;
        
        public UpdateSubMajorHeadHandler(BudgetContext context)
        {
            _context = context;
        }
        
        public async Task<Response> Handle(UpdateSubMajorHeadCommand request, CancellationToken ct)
        {
            var subMajorHead = await _context.SubMajorHeads
                .FirstOrDefaultAsync(s => s.SubMajorAccCode == request.SubMajorAccCode, ct);
                
            if (subMajorHead == null)
                return new Response 
                { 
                    IsSucced = false, 
                    Message = $"Sub-major head with code '{request.SubMajorAccCode}' not found" 
                };
            
            // Update fields
            subMajorHead.SubMajorAccDesc = request.dto.SubMajorAccDesc;
            subMajorHead.MajorAccCode = request.dto.MajorAccCode;
            subMajorHead.UserCode = request.dto.UserCode;
            subMajorHead.ModificationDate = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);
            
            return new Response 
            { 
                IsSucced = true, 
                Message = "Sub-major head updated successfully" 
            };
        }
    }
}
