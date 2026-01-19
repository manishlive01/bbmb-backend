using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Classes.Update
{
    public class UpdateClassHandler : IRequestHandler<UpdateClassCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateClassHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateClassCommand request, CancellationToken ct)
        {
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.ClassCode == request.Code, ct);

            if (classEntity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Class with code '{request.Code}' not found"
                };
            }

            classEntity.ClassDesc = request.dto.ClassDesc;
            classEntity.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Class updated successfully"
            };
        }
    }
}
