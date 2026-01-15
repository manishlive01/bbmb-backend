using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById
{
    public class GetMajorHeadByIdHandler
        : IRequestHandler<GetMajorHeadByIdCommand, GetMajorHeadByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetMajorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetMajorHeadByIdResponseDto?> Handle(
            GetMajorHeadByIdCommand request,
            CancellationToken ct)
        {

            return await _context.MajorHeads
                .AsNoTracking()
                .Where(x => x.MajorAccCode == request.Request.Id)
                .Select(x => new GetMajorHeadByIdResponseDto
                {
                    MajorAccCode = x.MajorAccCode,
                    MajorAccDesc = x.MajorAccDesc
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
