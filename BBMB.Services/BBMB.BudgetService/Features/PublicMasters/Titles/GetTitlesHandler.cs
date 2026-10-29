using BBMB.BudgetService.Features.PublicMasters.Titles.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.Titles
{
    public class GetTitleListHandler
        : IRequestHandler<GetTitleListCommand, List<GetTitleListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetTitleListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetTitleListResponseDto>> Handle(
            GetTitleListCommand request,
            CancellationToken ct)
        {
            return await _context.Titles
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.TitleName)
                .Select(x => new GetTitleListResponseDto
                {
                    Id = x.Id,
                    TitleName = x.TitleName
                })
                .ToListAsync(ct);
        }
    }
}