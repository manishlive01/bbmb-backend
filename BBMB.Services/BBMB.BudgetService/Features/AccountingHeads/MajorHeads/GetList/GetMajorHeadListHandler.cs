using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList
{
    public class GetMajorHeadListHandler
        : IRequestHandler<GetMajorHeadListCommand, List<GetMajorHeadListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetMajorHeadListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetMajorHeadListResponseDto>> Handle(
            GetMajorHeadListCommand request,
            CancellationToken ct)
        {
            return await _context.MajorHeads
                .AsNoTracking()
                .OrderBy(x => x.MajorAccCode)
                .Select(x => new GetMajorHeadListResponseDto
                {
                    MajorAccCode = x.MajorAccCode,
                    MajorAccDesc = x.MajorAccDesc,
                    ModificationDate = x.ModificationDate,
                    UserCode = x.UserCode
                })
                .ToListAsync(ct);
        }
    }
}
