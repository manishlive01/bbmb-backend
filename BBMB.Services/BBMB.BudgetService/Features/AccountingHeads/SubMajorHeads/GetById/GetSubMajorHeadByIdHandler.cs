using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public class GetSubMajorHeadByIdHandler
        : IRequestHandler<GetSubMajorHeadByIdCommand, GetSubMajorHeadByIdResponseDto?>
    {
        private readonly BudgetContext _context;

        public GetSubMajorHeadByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetSubMajorHeadByIdResponseDto?> Handle(
            GetSubMajorHeadByIdCommand request,
            CancellationToken ct)
        {
            return await _context.SubMajorHeads
                .AsNoTracking()
                .Where(x =>
                    x.SubMajorAccCode == request.Request.Id)
                .Select(x => new GetSubMajorHeadByIdResponseDto
                {
                    MajorAccCode = x.MajorAccCode,
                    SubMajorAccCode = x.SubMajorAccCode,
                    SubMajorAccDesc = x.SubMajorAccDesc
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}
