using BBMB.BudgetService.Features.ObjectClassifications.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetById
{
    public class GetObjectClassificationByIdHandler : IRequestHandler<GetObjectClassificationByIdQuery, GetObjectClassificationByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetObjectClassificationByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetObjectClassificationByIdResponseDto> Handle(GetObjectClassificationByIdQuery request, CancellationToken ct)
        {
            var objectClassification = await _context.ObjectClassifications
                .FirstOrDefaultAsync(o => o.ObjectAccCode == request.Code, ct);

            if (objectClassification == null)
            {
                throw new KeyNotFoundException($"Object classification with code '{request.Code}' not found");
            }

            return objectClassification.Adapt<GetObjectClassificationByIdResponseDto>();
        }
    }
}
