using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.EmployeeCharges.Update
{
    public class UpdateEmployeeChargeHandler
        : IRequestHandler<UpdateEmployeeChargeCommand, Response>
    {
        private readonly EmployeeContext _context;

        public UpdateEmployeeChargeHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateEmployeeChargeCommand request, CancellationToken ct)
        {
            var dto = request.dto;

            var employeeCharge = await _context.EmployeeCharges
                .FirstOrDefaultAsync(x => x.Id == dto.Id, ct);

            if (employeeCharge == null)
            {
                return new Response<Guid>
                {
                    IsSucced = false,
                    Message = "Employee charge not found"
                };
            }

            employeeCharge.EmployeeId = dto.EmployeeId;
            employeeCharge.ProjectCode = dto.ProjectCode;
            employeeCharge.CECode = dto.CECode;
            employeeCharge.CRCode = dto.CRCode;
            employeeCharge.DivisionCode = dto.DivisionCode;
            employeeCharge.ChargeTime = dto.ChargeTime;
            employeeCharge.ChargeExpireTime = dto.ChargeExpireTime;
            employeeCharge.IsActive = dto.IsActive;
            employeeCharge.IsChargeExpire = dto.IsChargeExpire;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Employee charge updated successfully"
            };
        }
    }
}