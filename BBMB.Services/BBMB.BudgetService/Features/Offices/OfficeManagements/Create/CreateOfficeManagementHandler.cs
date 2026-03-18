using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.OfficeManagements.Create
{
    public class CreateOfficeManagementHandler
        : IRequestHandler<CreateOfficeManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateOfficeManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateOfficeManagementCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            var exists = await _context.OfficeManagements
                .AnyAsync(x => x.OfficeCode == dto.OfficeCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Office already exists"
                };
            }

            var entity = dto.Adapt<OfficeManagement>();

            entity.ModificationDate = DateTime.UtcNow;

            await _context.OfficeManagements.AddAsync(entity, ct);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Office management created successfully"
            };
        }
    }
}