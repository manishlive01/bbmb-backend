using BBMB.BudgetService.Features.PublicMasters.Genders.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.Genders
{
    public class GetGenderListHandler
    : IRequestHandler<GetGenderListCommand, List<GetGenderListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetGenderListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetGenderListResponseDto>> Handle(
            GetGenderListCommand request,
            CancellationToken ct)
        {
            return await _context.Genders
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.GenderName)
                .Select(x => new GetGenderListResponseDto
                {
                    Id = x.Id,
                    GenderName = x.GenderName
                })
                .ToListAsync(ct);
        }
    }
}
