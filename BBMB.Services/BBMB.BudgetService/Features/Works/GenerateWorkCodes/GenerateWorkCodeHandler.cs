using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Features.Works.GenerateWorkCodes.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GenerateWorkCodes
{
    public class GenerateWorkCodeHandler
        : IRequestHandler<GenerateWorkCodeQuery, Response<GenerateWorkCodeResponseDto>>
    {
        private readonly BudgetContext _context;

        public GenerateWorkCodeHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response<GenerateWorkCodeResponseDto>> Handle(
            GenerateWorkCodeQuery request,
            CancellationToken cancellationToken)
        {
            // P4 (design §4.6, BUD-232): a free work-code generator cannot exist — a code is generated only
            // on the four allocation occasions by budget.fn_generate_work_code, never on demand. This endpoint
            // and its slice are removed in P4-03; until then it refuses rather than issue a non-conformant code.
            return new Response<GenerateWorkCodeResponseDto>
            {
                IsSucced = false,
                Message = "Work-code generation on demand is not permitted (BUD-232): a code is assigned only on allocation."
            };
        }
    }
}