using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Projects.Delete
{
    public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, Response>
    {
        private readonly BudgetContext _context;

        public DeleteProjectHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(DeleteProjectCommand request, CancellationToken ct)
        {
            var project = await _context.Projects
                .Include(p => p.UnitMasters)
                .FirstOrDefaultAsync(p => p.ProjCode == request.Code, ct);

            if (project == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Project with code '{request.Code}' not found"
                };
            }

            if (project.UnitMasters.Any())
            {
                var unitCodes = string.Join(", ", project.UnitMasters.Select(u => u.UnitCode));
                return new Response
                {
                    IsSucced = false,
                    Message = $"Cannot delete project. Associated units: {unitCodes}"
                };
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Project deleted successfully"
            };
        }
    }
}
