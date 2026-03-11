using BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments
{
    public class GetSourceOfRecruitmentListHandler
     : IRequestHandler<GetSourceOfRecruitmentListCommand, List<GetSourceOfRecruitmentListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetSourceOfRecruitmentListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetSourceOfRecruitmentListResponseDto>> Handle(
            GetSourceOfRecruitmentListCommand request,
            CancellationToken ct)
        {
            return await _context.SourceOfRecruitments
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.SourceName)
                .Select(x => new GetSourceOfRecruitmentListResponseDto
                {
                    Id = x.Id,
                    SourceName = x.SourceName
                })
                .ToListAsync(ct);
        }
    }
}
