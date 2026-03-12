using BBMB.BudgetService.Features.EmployeeCharges.GetByUser;
using BBMB.BudgetService.Features.EmployeeCharges.GetByUser.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetList
{
    public class GetEmployeeChargeHandler
        : IRequestHandler<GetEmployeeChargeQuery, List<GetEmployeeChargeResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetEmployeeChargeHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetEmployeeChargeResponseDto>> Handle(
            GetEmployeeChargeQuery request,
            CancellationToken ct)
        {
            return await _context.EmployeeCharges
                .AsNoTracking()
                .Select(ec => new GetEmployeeChargeResponseDto
                {
                    Id = ec.Id,
                    EmployeeId = ec.EmployeeId,
                    ProjectCode = ec.ProjectCode,
                    CECode = ec.CECode,
                    CRCode = ec.CRCode,
                    DivisionCode = ec.DivisionCode,
                    ChargeTime = ec.ChargeTime,
                    ChargeExpireTime = ec.ChargeExpireTime,
                    IsActive = ec.IsActive,
                    IsChargeExpire = ec.IsChargeExpire
                })
                .ToListAsync(ct);
        }
    }
}