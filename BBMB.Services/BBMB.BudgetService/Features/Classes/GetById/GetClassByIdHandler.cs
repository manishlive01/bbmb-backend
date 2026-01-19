using BBMB.BudgetService.Features.Classes.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Classes.GetById
{
    public class GetClassByIdHandler : IRequestHandler<GetClassByIdQuery, GetClassByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetClassByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetClassByIdResponseDto> Handle(GetClassByIdQuery request, CancellationToken ct)
        {
            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.ClassCode == request.Code, ct);

            if (classEntity == null)
            {
                throw new KeyNotFoundException($"Class with code '{request.Code}' not found");
            }

            return new GetClassByIdResponseDto
            {
                ClassCode = classEntity.ClassCode,
                ClassDesc = classEntity.ClassDesc,
                ModificationDate = classEntity.ModificationDate
            };
        }
    }
}
