using BBMB.BudgetService.Features.PublicMasters.Designations.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.Designations
{
    public class GetDesignationListHandler
     : IRequestHandler<GetDesignationListCommand, List<GetDesignationListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetDesignationListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetDesignationListResponseDto>> Handle(
            GetDesignationListCommand request,
            CancellationToken ct)
        {
            return await _context.Designations
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.DesignationName)
                .Select(x => new GetDesignationListResponseDto
                {
                    Id = x.Id,
                    DesignationName = x.DesignationName
                })
                .ToListAsync(ct);
        }
    }
}