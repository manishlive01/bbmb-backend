using BBMB.BudgetService.Features.Estimates.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Estimates.GetById
{
    public class GetEstimateByIdHandler : IRequestHandler<GetEstimateByIdQuery, Response<GetEstimateByIdResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetEstimateByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<GetEstimateByIdResponseDto>> Handle(GetEstimateByIdQuery request, CancellationToken cancellationToken)
        {
#pragma warning disable CS0618 // work_code retained read-only through P4 (design §4.6 step 2, OI-46)
            var estimate = await _context.Estimates
                .Where(e => e.WorkId == request.WorkId && e.EstimateCode == request.EstimateCode)
                .Select(e => new GetEstimateByIdResponseDto
                {
                    WorkId = e.WorkId,
                    WorkCode = e.WorkCode,
                    EstimateCode = e.EstimateCode,
                    EstimateDesc = e.EstimateDesc,
                    EstimateAmt = e.EstimateAmt,
                    EstimateNarr = e.EstimateNarr,
                    FinYearEnd = e.FinYearEnd,
                    DivisionCode = e.DivisionCode,
                    WorkDesc = e.Work != null ? e.Work.WorkDesc : null,
                    DivisionDesc = _context.Divisions
                        .Where(d => d.DivisionCode == e.DivisionCode)
                        .Select(d => d.DivisionDesc)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);
#pragma warning restore CS0618

            if (estimate == null)
            {
                return new Response<GetEstimateByIdResponseDto>
                {
                    IsSucced = false,
                    Message = "Estimate not found"
                };
            }

            return new Response<GetEstimateByIdResponseDto>
            {
                IsSucced = true,
                Message = "Estimate retrieved successfully",
                Data = estimate
            };
        }
    }
}
