using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationHandler : IRequestHandler<UpdateDesignationCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDesignationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateDesignationCommand request, CancellationToken ct)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(d => d.DesignationCode == request.Code, ct);

            if (designation == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Designation with code '{request.Code}' not found"
                };
            }

            designation.DesignationDesc = request.dto.DesignationDesc;
            designation.UserCode = request.dto.UserCode;
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
