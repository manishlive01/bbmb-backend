using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public class GetSubMajorHeadListHandler
        : IRequestHandler<GetSubMajorHeadListCommand, List<GetSubMajorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetSubMajorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetSubMajorHeadListResponseDto>> Handle(
            GetSubMajorHeadListCommand request,
            CancellationToken ct)
        {
            return await _context.SubMajorHeads
                .AsNoTracking()
                .OrderBy(x => x.MajorAccCode)
                .ThenBy(x => x.SubMajorAccCode)
                .Select(x => new GetSubMajorHeadListResponseDto
                {
                    MajorAccCode = x.MajorAccCode,
                    SubMajorAccCode = x.SubMajorAccCode,
                    SubMajorAccDesc = x.SubMajorAccDesc,
                    ModificationDate = x.ModificationDate.ToString("dd-MM-yyyy"),
                    UserCode = x.UserCode
                })
                .ToListAsync(ct);
        }
    }
}
