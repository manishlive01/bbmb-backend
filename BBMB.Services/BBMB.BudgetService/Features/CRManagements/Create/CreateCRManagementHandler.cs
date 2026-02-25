using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CRManagements.Create
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
                .AnyAsync(x => x.CECode == request.Dto.CECode, ct);

            if (!ceExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"CE with code '{request.Dto.CECode}' not found"
                };
            }

            // 🔐 Get user from JWT
            //var userCode = JwtClaimHelper.GetUserId(_http.HttpContext!.User);

            var cr = new CRManagement
            {
                CRCode = request.Dto.CRCode,
                CECode = request.Dto.CECode,
                Name = request.Dto.Name,
                Description = request.Dto.Description,
                Location = request.Dto.Location,

                // 🔥 Backend Controlled
                Status = request.Dto.Status,
                Date = DateTime.UtcNow,
                UserCode = "Usr123"
            };


            _context.CRManagements.Add(cr);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "CR Management created successfully"
            };
        }
    }
}
