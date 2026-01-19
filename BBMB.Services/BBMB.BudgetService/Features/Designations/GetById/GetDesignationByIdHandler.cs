using BBMB.BudgetService.Features.Designations.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public class GetDesignationByIdHandler : IRequestHandler<GetDesignationByIdQuery, GetDesignationByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetDesignationByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDesignationByIdResponseDto> Handle(GetDesignationByIdQuery request, CancellationToken ct)
        {
            var designation = await _context.Designations
                .FirstOrDefaultAsync(d => d.DesignationCode == request.Code, ct);

            if (designation == null)
            {
                throw new KeyNotFoundException($"Designation with code '{request.Code}' not found");
            }

            return designation.Adapt<GetDesignationByIdResponseDto>();
        }
    }
}
