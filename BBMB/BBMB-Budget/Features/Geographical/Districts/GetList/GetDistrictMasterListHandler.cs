using BBMB_Budget.Features.Geographical.Districts.GetList.Dtos;
using BBMB_Budget.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Features.Geographical.Districts.GetList
{
    public class GetDistrictMasterListHandler
        : IRequestHandler<GetDistrictMasterListCommand, List<GetDistrictMasterListResponseDto>>
    {
        private readonly BBMBContext _context;

        public GetDistrictMasterListHandler(BBMBContext context)
        {
            _context = context;
        }

        public async Task<List<GetDistrictMasterListResponseDto>> Handle(
            GetDistrictMasterListCommand request,
            CancellationToken ct)
        {
            return await _context.DistrictMasters
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.DistrictName)
                .Select(x => new GetDistrictMasterListResponseDto
                {
                    DistrictMasterId = x.DistrictMasterId,
                    StateMasterId = x.StateMasterId,
                    StateName = x.StateMasters.StateName,
                    DistrictCode = x.DistrictCode,
                    DistrictName = x.DistrictName,
                    IsActive = x.IsActive
                })
                .ToListAsync(ct);
        }
    }
}
