using BBMB.BudgetService.Features.EmployeeCharges.GetListByUser.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.Utilities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser
{
    public class GetEmployeeChargeByUserHandler
        : IRequestHandler<GetEmployeeChargeByUserQuery, List<GetEmployeeChargeByUserResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetEmployeeChargeByUserHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetEmployeeChargeByUserResponseDto>> Handle(
            GetEmployeeChargeByUserQuery request,
            CancellationToken cancellationToken)
        {
            // 🔹 Get UserId from Token
            var userId = JwtClaimHelper.GetUserId(request.User);

            if (string.IsNullOrEmpty(userId))
                return new List<GetEmployeeChargeByUserResponseDto>();

            var userGuid = Guid.Parse(userId);

            // 🔹 Join Employees → EmployeeCharges
            return await (
                from ec in _context.EmployeeCharges
                join e in _context.Employees
                    on ec.EmployeeId equals e.Id
                where e.UserId == userGuid
                select new GetEmployeeChargeByUserResponseDto
                {
                    ProjectId = ec.ProjectId,
                    CECode = ec.CECode,
                    CRCode = ec.CRCode,
                    DivCode = ec.DivCode
                }
            ).AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}