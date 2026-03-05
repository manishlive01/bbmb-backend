using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public class CreateEmployeeChargeHandler
         : IRequestHandler<CreateEmployeeChargeCommand, Response>
    {
        private readonly EmployeeContext _context;

        public CreateEmployeeChargeHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(
            CreateEmployeeChargeCommand request,
            CancellationToken ct)
        {
            var dto = request.dto;

            var employeeExists = await _context.Employees
                .AnyAsync(x => x.Id == dto.EmployeeId, ct);

            if (!employeeExists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Employee not found"
                };
            }

            var charge = dto.Adapt<EmployeeCharge>();
            charge.Id = Guid.NewGuid();

            await _context.EmployeeCharges.AddAsync(charge, ct);

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee charge created successfully"
            };
        }
    }
}
