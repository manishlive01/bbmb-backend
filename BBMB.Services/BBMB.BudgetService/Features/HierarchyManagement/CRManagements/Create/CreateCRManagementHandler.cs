using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Create
{
    public class CreateCRManagementHandler
        : IRequestHandler<CreateCRManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateCRManagementHandler(
            BudgetContext context
            )
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateCRManagementCommand request, CancellationToken ct)
        {
            // ✅ Validate CE exists
            var ceExists = await _context.CEManagements
                .FirstOrDefaultAsync(x => x.Id == request.Dto.CEId, ct);

            if (ceExists is null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE not found"
                };
            }

            // 🔐 Get user from JWT
            //var userCode = JwtClaimHelper.GetUserId(_http.HttpContext!.User);

            var cr = new CRManagement
            {
                CRCode = request.Dto.CRCode,
                CEId= ceExists.Id, 
                Name = request.Dto.Name,
                Description = request.Dto.Description,
                Location = request.Dto.Location,

                // 🔥 Backend Controlled
                Status = request.Dto.Status,
                Date = DateTime.UtcNow,
                UserCode = "Usr123"
            };


            await _context.CRManagements.AddAsync(cr);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Management created successfully"
            };
        }
    }
}
