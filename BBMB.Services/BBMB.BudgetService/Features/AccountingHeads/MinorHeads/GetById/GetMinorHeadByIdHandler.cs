using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById
{
    public class GetMinorHeadByIdHandler
        : IRequestHandler<GetMinorHeadByIdCommand, GetMinorHeadByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetMinorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetMinorHeadByIdResponseDto?> Handle(
            GetMinorHeadByIdCommand request,
            CancellationToken ct)
        {

            return await _context.MinorHeads
                .AsNoTracking()
                .Where(x => x.MinorAccCode == request.Request.Id)
                .Select(x => new GetMinorHeadByIdResponseDto
                {
                    MinorAccCode = x.MinorAccCode,
                    MinorAccDesc = x.MinorAccDesc
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
