using BBMB.BudgetService.Features.PublicMasters.FamilyRelations.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.PublicMasters.FamilyRelations
{
    public class GetFamilyRelationListHandler
        : IRequestHandler<GetFamilyRelationListCommand, List<GetFamilyRelationListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetFamilyRelationListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<List<GetFamilyRelationListResponseDto>> Handle(
            GetFamilyRelationListCommand request,
            CancellationToken ct)
        {
            return await _context.FamilyRelations
                .AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsStatus)
                .OrderBy(x => x.FamilyRelationName)
                .Select(x => new GetFamilyRelationListResponseDto
                {
                    Id = x.Id,
                    FamilyRelationName = x.FamilyRelationName
                })
                .ToListAsync(ct);
        }
    }
}