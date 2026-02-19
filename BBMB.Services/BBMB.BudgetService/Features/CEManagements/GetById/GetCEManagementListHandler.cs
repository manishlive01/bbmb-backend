using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.CEManagements.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CEManagements.GetById
{
    public class GetCEManagementByIdHandler
        : IRequestHandler<GetCEManagementByIdQuery, GetCEManagementByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetCEManagementByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetCEManagementByIdResponseDto> Handle(
            GetCEManagementByIdQuery request,
            CancellationToken ct)
        {
            var ce = await _context.CEManagements
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (ce == null)
            {
                throw new KeyNotFoundException(
                    $"CE Management record with ID '{request.Id}' not found");
            }

            // Map using Mapster
            return ce.Adapt<GetCEManagementByIdResponseDto>();
        }
    }
}
