using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById
{
    public class GetSubMinorHeadByIdHandler
        : IRequestHandler<GetSubMinorHeadByIdCommand, GetSubMinorHeadByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetSubMinorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetSubMinorHeadByIdResponseDto?> Handle(
            GetSubMinorHeadByIdCommand request,
            CancellationToken ct)
        {
            return await _context.SubMinorHeads
                .AsNoTracking()
                .Where(x => x.SubMinorAccCode == request.Request.Id)// SubMinorAccCode
                .Select(x => new GetSubMinorHeadByIdResponseDto
                {
                    MinorAccCode = x.MinorAccCode,
                    SubMinorAccCode = x.SubMinorAccCode,
                    SubMinorAccDesc = x.SubMinorAccDesc
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
