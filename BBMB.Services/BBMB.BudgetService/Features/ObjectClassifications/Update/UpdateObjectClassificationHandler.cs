using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.ObjectClassifications.Update
{
    public class UpdateObjectClassificationHandler : IRequestHandler<UpdateObjectClassificationCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateObjectClassificationHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateObjectClassificationCommand request, CancellationToken ct)
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

            objectClassification.ObjectAccDesc = request.dto.ObjectAccDesc;
            objectClassification.UserCode = request.dto.UserCode;
            objectClassification.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Object classification updated successfully"
            };
        }
    }
}
