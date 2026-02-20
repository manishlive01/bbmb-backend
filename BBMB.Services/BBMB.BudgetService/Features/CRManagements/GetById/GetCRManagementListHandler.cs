using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Features.CRManagements.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.CRManagements.GetById
{
    public class GetCRManagementByIdHandler
        : IRequestHandler<GetCRManagementByIdQuery, GetCRManagementByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetCRManagementByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetCRManagementByIdResponseDto> Handle(
            GetCRManagementByIdQuery request,
            CancellationToken ct)
        {
            var cr = await _context.CRManagements
                .Include(x => x.CEManagement)
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (cr == null)
            {
                throw new KeyNotFoundException(
                    $"CR Management record with ID '{request.Id}' not found");
            }

            return new GetCRManagementByIdResponseDto
            {
                CEId = cr.CEId,
                Name = cr.Name,
                Description = cr.Description,
                Location = cr.Location,
                Date = cr.Date,
                Status = cr.Status,
                UserCode = cr.UserCode
            };
        }
    }
}
