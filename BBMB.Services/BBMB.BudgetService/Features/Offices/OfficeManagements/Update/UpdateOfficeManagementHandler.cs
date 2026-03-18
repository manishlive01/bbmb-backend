using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update
{
    public class UpdateOfficeManagementHandler
        : IRequestHandler<UpdateOfficeManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateOfficeManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            UpdateOfficeManagementCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            var entity = await _context.OfficeManagements
                .FirstOrDefaultAsync(x => x.OfficeCode == dto.OfficeCode, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Office management not found"
                };
            }

            // Map values
            dto.Adapt(entity);

            entity.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Office management updated successfully"
            };
        }
    }
}