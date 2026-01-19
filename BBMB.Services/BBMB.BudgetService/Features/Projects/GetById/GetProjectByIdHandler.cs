using BBMB.BudgetService.Features.Projects.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Projects.GetById
{
    public class GetProjectByIdHandler : IRequestHandler<GetProjectByIdQuery, GetProjectByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetProjectByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetProjectByIdResponseDto> Handle(GetProjectByIdQuery request, CancellationToken ct)
        {
            var project = await _context.Projects
                .Include(p => p.UnitMasters)
                .FirstOrDefaultAsync(p => p.ProjCode == request.Code, ct);

            if (project == null)
            {
                throw new KeyNotFoundException($"Project with code '{request.Code}' not found");
            }

            return new GetProjectByIdResponseDto
            {
                ProjCode = project.ProjCode,
                ProjDesc = project.ProjDesc,
                ModificationDate = project.ModificationDate,
                UserCode = project.UserCode,
                AssociatedUnitsCount = project.UnitMasters.Count
            };
        }
    }
}
