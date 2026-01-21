using BBMB.BudgetService.Features.DetailHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.GetById
{
    public class GetDetailHeadByIdHandler : IRequestHandler<GetDetailHeadByIdCommand, GetDetailHeadByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetDetailHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDetailHeadByIdResponseDto> Handle(GetDetailHeadByIdCommand request, CancellationToken cancellationToken)
        {
            var detailHead = await _context.DetailHeads
                .FirstOrDefaultAsync(d => d.DetailAccCode == request.DetailAccCode, cancellationToken);

            if (detailHead == null)
            {
                throw new KeyNotFoundException($"Detail Head with code '{request.DetailAccCode}' not found.");
            }

            return new GetDetailHeadByIdResponseDto
            {
                DetailAccCode = detailHead.DetailAccCode,
                DetailAccDesc = detailHead.DetailAccDesc,
            };
        }
    }
}
