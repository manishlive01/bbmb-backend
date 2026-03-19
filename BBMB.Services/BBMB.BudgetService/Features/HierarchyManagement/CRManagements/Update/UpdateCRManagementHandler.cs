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
                .FirstOrDefaultAsync(x => x.CRCode == request.crCode && x.DivSection == request.Dto.DivSection, ct);

            if (cr == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CR Management record with ID '{request.crCode}' not found"
                };
            }

            // ✅ Validate CE exists
            var ceExists = await _context.CEManagements
                .AnyAsync(x => x.CECode == request.Dto.CECode, ct);

            if (!ceExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE with ID '{request.Dto.CECode}' not found"
                };
            }

            // 🔄 Update fields
            cr.CECode = request.Dto.CECode;
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
