using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.Create
{
    public class CreateDetailHeadHandler : IRequestHandler<CreateDetailHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateDetailHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateDetailHeadCommand request, CancellationToken cancellationToken)
        {
            // Check for duplicate
            var exists = await _context.DetailHeads
                .AnyAsync(d => d.DetailAccCode == request.dto.DetailAccCode, cancellationToken);

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Detail Head with code '{request.dto.DetailAccCode}' already exists."
                };
            }

            // Map and create entity
            var detailHead = request.dto.Adapt<DetailHead>();
            detailHead.ModificationDate = DateTime.UtcNow;

            _context.DetailHeads.Add(detailHead);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                IsSucced = true,
                Message = $"Detail Head '{detailHead.DetailAccCode}' created successfully."
            };
        }
    }
}
