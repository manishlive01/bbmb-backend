using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Update
{
    public class UpdateCEManagementHandler
        : IRequestHandler<UpdateCEManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateCEManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateCEManagementCommand request, CancellationToken ct)
        {
            var ce = await _context.CEManagements
                .FirstOrDefaultAsync(x => x.CECode == request.ceCode && x.DivSection == request.Dto.DivSection, ct);

            if (ce == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE Management record with Code '{request.ceCode}' not found"
                };
            }

            // 🔎 Optional: Check duplicate (if needed)
            var exists = await _context.CEManagements
                .AnyAsync(x => x.Name == request.Dto.Name && x.CECode != request.ceCode, ct);
            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "CE record already exists with this name."
                };
            }

            // 🔹 Map only allowed fields
            ce.Name = request.Dto.Name;
            ce.ProjectCode = request.Dto.ProjectCode;
            ce.Description = request.Dto.Description;
            ce.Location = request.Dto.Location;
            ce.Status = request.Dto.Status;

            // 🔥 Backend controlled fields
            ce.Date = DateTime.UtcNow;          // Auto update date
            ce.UserCode = "Usr123";             // Replace with logged-in user

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CE Management record updated successfully"
            };
        }

    }
}
