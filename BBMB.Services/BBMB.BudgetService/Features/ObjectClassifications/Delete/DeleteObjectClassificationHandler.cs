using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.ObjectClassifications.Delete
{
    public class DeleteObjectClassificationHandler : IRequestHandler<DeleteObjectClassificationCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteObjectClassificationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteObjectClassificationCommand request, CancellationToken ct)
        {
            var objectClassification = await _context.ObjectClassifications
                .FirstOrDefaultAsync(o => o.ObjectAccCode == request.Code, ct);

            if (objectClassification == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Object classification with code '{request.Code}' not found"
                };
            }

            _context.ObjectClassifications.Remove(objectClassification);

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Cannot delete object classification. It has associated records."
                };
            }

            return new Response
            {
                IsSucced = true,
                Message = "Object classification deleted successfully"
            };
        }
    }
}
