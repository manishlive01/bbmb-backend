using BBMB.BudgetService.Features.Projects.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Projects.Create
{
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateProjectHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateProjectCommand request, CancellationToken ct)
        {
            // Check for duplicate
            var exists = await _context.Projects
                .AnyAsync(p => p.ProjCode == request.dto.ProjCode, ct);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Project with code '{request.dto.ProjCode}' already exists"
                };
            }

            // Map DTO to Entity
            var project = request.dto.Adapt<Project>();
            project.ModificationDate = DateTime.UtcNow;

            // Persist
            _context.Projects.Add(project);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Project created successfully"
            };
        }
    }
}
