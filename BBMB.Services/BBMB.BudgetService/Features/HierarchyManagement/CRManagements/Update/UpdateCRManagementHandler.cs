using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Update
{
    public class UpdateCRManagementHandler
        : IRequestHandler<UpdateCRManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateCRManagementHandler(
            BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateCRManagementCommand request, CancellationToken ct)
        {
            var cr = await _context.CRManagements
                .FirstOrDefaultAsync(x => x.Id == request.crId  , ct);

            if (cr == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CR Management record  not found"
                };
            }

            // ✅ Validate CE exists
            var ceExists = await _context.CEManagements
                .AnyAsync(x => x.Id == request.Dto.CEId , ct);

            if (!ceExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE not found"
                };
            }

            // 🔄 Update fields
            cr.CEId = request.Dto.CEId;
            cr.Name = request.Dto.Name; 
            cr.Description = request.Dto.Description;
            cr.Location = request.Dto.Location;
            cr.Status = request.Dto.Status;

            // 🔐 Backend controlled fields
            cr.Date = DateTime.UtcNow;
            //cr.UserCode = JwtClaimHelper.GetUserId(_http.HttpContext!.User) ?? "SYSTEM";
            cr.UserCode = "Usr123";

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Management updated successfully"
            };
        }
    }
}
