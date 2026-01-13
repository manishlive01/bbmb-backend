using BBMB.BudgetService.Features.Geographical.Districts.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Geographical.Districts.GetById
{
    public class GetDistrictMasterByIdHandler
        : IRequestHandler<GetDistrictMasterByIdCommand, GetDistrictMasterByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetDistrictMasterByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDistrictMasterByIdResponseDto?> Handle(
            GetDistrictMasterByIdCommand request,
            CancellationToken ct)
        {
            return await _context.DistrictMasters
                .AsNoTracking()
                .Where(x =>
                    x.DistrictMasterId == request.Request.Id &&
                    !x.IsDeleted)
                .Select(x => new GetDistrictMasterByIdResponseDto
                {
                    StateMasterId = x.StateMasterId,
                    StateName = x.StateMasters.StateName,
                    DistrictCode = x.DistrictCode,
                    DistrictName = x.DistrictName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
