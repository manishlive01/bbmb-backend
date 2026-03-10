using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.Delete
{
    public class DeleteEmployeeChargeHandler
        : IRequestHandler<DeleteEmployeeChargeCommand, Response>
    {
        private readonly EmployeeContext _context;

        public DeleteEmployeeChargeHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteEmployeeChargeCommand request, CancellationToken ct)
        {
            var employeeCharge = await _context.EmployeeCharges
                .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

            if (employeeCharge == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee charge not found"
                };
            }

            _context.EmployeeCharges.Remove(employeeCharge);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee charge deleted successfully"
            };
        }
    }
}