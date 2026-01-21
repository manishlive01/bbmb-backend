using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.Update
{
    public class UpdateDetailHeadHandler : IRequestHandler<UpdateDetailHeadCommand, Response>
    {
        private readonly BudgetContext _context;

        public UpdateDetailHeadHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(UpdateDetailHeadCommand request, CancellationToken cancellationToken)
        {
            // Find the entity
            var detailHead = await _context.DetailHeads
                .FirstOrDefaultAsync(d => d.DetailAccCode == request.DetailAccCode, cancellationToken);

            if (detailHead == null)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Detail Head with code '{request.DetailAccCode}' not found."
                };
            }

            // Update fields
            detailHead.DetailAccDesc = request.dto.DetailAccDesc;
            //detailHead.MajorAccCode = request.dto.MajorAccCode;
            //detailHead.SubMajorAccCode = request.dto.SubMajorAccCode;
            //detailHead.MinorAccCode = request.dto.MinorAccCode;
            //detailHead.SubMinorAccCode = request.dto.SubMinorAccCode;
            detailHead.UserCode = "123";
            detailHead.ModificationDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                IsSucced = true,
                Message = $"Detail Head '{detailHead.DetailAccCode}' updated successfully."
            };
        }
    }
}
