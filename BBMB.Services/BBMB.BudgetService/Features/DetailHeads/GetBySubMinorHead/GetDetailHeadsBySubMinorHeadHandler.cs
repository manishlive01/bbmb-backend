using BBMB.BudgetService.Features.DetailHeads.GetBySubMinorHead.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.GetBySubMinorHead
{
    public class GetDetailHeadsBySubMinorHeadHandler : IRequestHandler<GetDetailHeadsBySubMinorHeadQuery, List<GetDetailHeadsBySubMinorHeadResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetDetailHeadsBySubMinorHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<List<GetDetailHeadsBySubMinorHeadResponseDto>> Handle(GetDetailHeadsBySubMinorHeadQuery request, CancellationToken cancellationToken)
        {
            //var detailHeads = await _context.DetailHeads
            //    .Include(d => d.MajorHead)
            //    .Include(d => d.SubMajorHead)
            //    .Include(d => d.MinorHead)
            //    .Include(d => d.SubMinorHead)
            //    .Where(d => d.SubMinorAccCode == request.SubMinorAccCode)
            //    .Select(d => new GetDetailHeadsBySubMinorHeadResponseDto
            //    {
            //        DetailAccCode = d.DetailAccCode,
            //        DetailAccDesc = d.DetailAccDesc,
            //        MajorAccCode = d.MajorAccCode,
            //        MajorAccDesc = d.MajorHead != null ? d.MajorHead.MajorAccDesc : string.Empty,
            //        SubMajorAccCode = d.SubMajorAccCode,
            //        SubMajorAccDesc = d.SubMajorHead != null ? d.SubMajorHead.SubMajorAccDesc : string.Empty,
            //        MinorAccCode = d.MinorAccCode,
            //        MinorAccDesc = d.MinorHead != null ? d.MinorHead.MinorAccDesc : string.Empty,
            //        SubMinorAccCode = d.SubMinorAccCode,
            //        SubMinorAccDesc = d.SubMinorHead != null ? d.SubMinorHead.SubMinorAccDesc : string.Empty,
            //        UserCode = d.UserCode,
            //        ModificationDate = d.ModificationDate
            //    })
            //    .ToListAsync(cancellationToken);

            return null;
        }
    }
}
