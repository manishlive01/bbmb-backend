using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Delete
{
    public class DeleteOfficeManagementHandler
        : IRequestHandler<DeleteOfficeManagementCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteOfficeManagementHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            DeleteOfficeManagementCommand request,
            CancellationToken ct)
        {
            var entity = await _context.OfficeManagements
                .FirstOrDefaultAsync(x => x.OfficeCode == request.OfficeCode, ct);

            if (entity == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Office management not found"
                };
            }

            _context.OfficeManagements.Remove(entity);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Office management deleted successfully"
            };
        }
    }
}