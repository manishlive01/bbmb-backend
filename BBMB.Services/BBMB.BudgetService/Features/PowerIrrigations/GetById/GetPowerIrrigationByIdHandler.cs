using BBMB.BudgetService.Features.PowerIrrigations.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PowerIrrigations.GetById
{
    public class GetPowerIrrigationByIdHandler : IRequestHandler<GetPowerIrrigationByIdQuery, GetPowerIrrigationByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetPowerIrrigationByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetPowerIrrigationByIdResponseDto> Handle(GetPowerIrrigationByIdQuery request, CancellationToken ct)
        {
            var powerIrrigation = await _context.PowerIrrigations
                .FirstOrDefaultAsync(p => p.IrrPwrCode == request.Code, ct);

            if (powerIrrigation == null)
            {
                throw new KeyNotFoundException($"Power/Irrigation code '{request.Code}' not found");
            }

            return new GetPowerIrrigationByIdResponseDto
            {
                IrrPwrCode = powerIrrigation.IrrPwrCode,
                IrrPwrDesc = powerIrrigation.IrrPwrDesc,
                ModificationDate = powerIrrigation.ModificationDate,
                UserCode = powerIrrigation.UserCode
            };
        }
    }
}
