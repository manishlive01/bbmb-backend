using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Classes.Delete
{
    public class DeleteClassHandler : IRequestHandler<DeleteClassCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteClassHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteClassCommand request, CancellationToken ct)
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

            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Class deleted successfully"
            };
        }
    }
}
