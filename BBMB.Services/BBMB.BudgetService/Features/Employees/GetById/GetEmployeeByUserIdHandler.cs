using BBMB.BudgetService.Features.Employees.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public class GetEmployeeByUserIdHandler
         : IRequestHandler<GetEmployeeByUserIdQuery, GetEmployeeResponseDto?>
    {
        private readonly EmployeeContext _context;

        public GetEmployeeByUserIdHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<GetEmployeeResponseDto?> Handle(
            GetEmployeeByUserIdQuery request,
            CancellationToken ct)
        {
            var employee = await _context.Employees
                .Include(x => x.EmploymentDetail)
                .Include(x => x.PhotoDocument)
                .Include(x => x.FamilyMembers)
                .FirstOrDefaultAsync(x => x.UserId == request.UserId, ct);

            if (employee == null)
                return null;

            return employee.Adapt<GetEmployeeResponseDto>();
        }
    }
}
