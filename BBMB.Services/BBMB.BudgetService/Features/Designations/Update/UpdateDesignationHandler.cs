using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationHandler
        : IRequestHandler<UpdateDesignationCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDesignationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateDesignationCommand request,
            CancellationToken ct)
        {
            var dto = request.Dto;

            var designation = await _context.Designations
                .FirstOrDefaultAsync(
                    d => d.DesignationCode == dto.DesignationCode,
                    ct);

            if (designation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Designation with code '{dto.DesignationCode}' not found"
                };
            }

            // 🔄 Update fields
            designation.DesignationDesc = dto.DesignationDesc;
            designation.ClassCode = dto.ClassCode;
            designation.MinBasic = dto.MinBasic;
            designation.MaxBasic = dto.MaxBasic;
            designation.UserCode = " ";
            designation.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Designation updated successfully"
            };
        }
    }
}
