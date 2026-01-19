using BBMB.BudgetService.Features.Divisions.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Divisions.GetById
{
    public class GetDivisionByIdHandler : IRequestHandler<GetDivisionByIdQuery, GetDivisionByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetDivisionByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDivisionByIdResponseDto> Handle(GetDivisionByIdQuery request, CancellationToken ct)
        {
            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.DivisionCode == request.Code, ct);

            if (division == null)
            {
                throw new KeyNotFoundException($"Division with code '{request.Code}' not found");
            }

            return division.Adapt<GetDivisionByIdResponseDto>();
        }
    }
}
