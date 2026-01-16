using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList
{
    public class GetSubMinorHeadListHandler
        : IRequestHandler<GetSubMinorHeadListCommand, List<GetSubMinorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetSubMinorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetSubMinorHeadListResponseDto>> Handle(
            GetSubMinorHeadListCommand request,
            CancellationToken ct)
        {
            return await _context.SubMinorHeads
                .AsNoTracking()
                .Include(x => x.MinorHead) // ✅ Join with MinorHead
                .OrderBy(x => x.MinorAccCode)
                .ThenBy(x => x.SubMinorAccCode)
                .Select(x => new GetSubMinorHeadListResponseDto
                {
                    MinorAccCode = x.MinorAccCode,
                    SubMinorAccCode = x.SubMinorAccCode,
                    SubMinorAccDesc = x.SubMinorAccDesc,

                    // From parent Minor Head
                    MinorAccDesc = x.MinorHead.MinorAccDesc,

                    // UI date format
                    ModificationDate = x.ModificationDate.ToString("dd-MM-yyyy"),

                    UserCode = x.UserCode
                })
                .ToListAsync(ct);
        }
    }
}
