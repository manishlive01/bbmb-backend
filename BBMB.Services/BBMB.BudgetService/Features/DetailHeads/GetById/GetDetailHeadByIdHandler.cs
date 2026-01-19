using BBMB.BudgetService.Features.DetailHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.DetailHeads.GetById
{
    public class GetDetailHeadByIdHandler : IRequestHandler<GetDetailHeadByIdQuery, GetDetailHeadByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetDetailHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetDetailHeadByIdResponseDto> Handle(GetDetailHeadByIdQuery request, CancellationToken cancellationToken)
        {
            var detailHead = await _context.DetailHeads
                .Include(d => d.MajorHead)
                .Include(d => d.SubMajorHead)
                .Include(d => d.MinorHead)
                .Include(d => d.SubMinorHead)
                .FirstOrDefaultAsync(d => d.DetailAccCode == request.DetailAccCode, cancellationToken);

            if (detailHead == null)
            {
                throw new KeyNotFoundException($"Detail Head with code '{request.DetailAccCode}' not found.");
            }

            return new GetDetailHeadByIdResponseDto
            {
                DetailAccCode = detailHead.DetailAccCode,
                DetailAccDesc = detailHead.DetailAccDesc,
                MajorAccCode = detailHead.MajorAccCode,
                MajorAccDesc = detailHead.MajorHead?.MajorAccDesc ?? string.Empty,
                SubMajorAccCode = detailHead.SubMajorAccCode,
                SubMajorAccDesc = detailHead.SubMajorHead?.SubMajorAccDesc ?? string.Empty,
                MinorAccCode = detailHead.MinorAccCode,
                MinorAccDesc = detailHead.MinorHead?.MinorAccDesc ?? string.Empty,
                SubMinorAccCode = detailHead.SubMinorAccCode,
                SubMinorAccDesc = detailHead.SubMinorHead?.SubMinorAccDesc ?? string.Empty,
                UserCode = detailHead.UserCode,
                ModificationDate = detailHead.ModificationDate
            };
        }
    }
}
