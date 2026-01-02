using BBMB_Budget.Features.Geographical.Districts.GetById.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.Districts.GetById
{
    public class GetDistrictMasterByIdHandler
        : IRequestHandler<GetDistrictMasterByIdCommand, GetDistrictMasterByIdResponseDto?>
    {
        private readonly BBMBContext _context;

        public GetDistrictMasterByIdHandler(BBMBContext context)
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
                    StateName = x.StateMaster.StateName,
                    DistrictCode = x.DistrictCode,
                    DistrictName = x.DistrictName,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
