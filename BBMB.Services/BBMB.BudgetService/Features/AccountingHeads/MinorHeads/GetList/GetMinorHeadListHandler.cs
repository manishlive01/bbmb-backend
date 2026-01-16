using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public class GetMinorHeadListHandler
        : IRequestHandler<GetMinorHeadListCommand, List<GetMinorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetMinorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetMinorHeadListResponseDto>> Handle(
            GetMinorHeadListCommand request,
            CancellationToken ct)
        {
            return await _context.MinorHeads
                .AsNoTracking()
                .Include(x => x.SubMinorHeads)   // ✅ Include SubMinorHeads
                .OrderBy(x => x.MinorAccCode)
                .Select(x => new GetMinorHeadListResponseDto
                {
                    MinorAccCode = x.MinorAccCode,
                    MinorAccDesc = x.MinorAccDesc,
                    ModificationDate = x.ModificationDate.ToString("dd-MM-yyyy"),
                    UserCode = x.UserCode,

                    // ✅ Count SubMinorHeads linked to this MinorAccCode
                    SubMinorCount = x.SubMinorHeads.Count
                })
                .ToListAsync(ct);
        }
    }
}
