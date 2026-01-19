using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Projects.Update
{
    public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateProjectHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateProjectCommand request, CancellationToken ct)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjCode == request.Code, ct);

            if (project == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Project with code '{request.Code}' not found"
                };
            }

            project.ProjDesc = request.dto.ProjDesc;
            project.UserCode = request.dto.UserCode;
            project.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Project updated successfully"
            };
        }
    }
}
