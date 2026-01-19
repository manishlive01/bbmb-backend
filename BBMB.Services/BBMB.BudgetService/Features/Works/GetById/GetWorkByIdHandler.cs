using BBMB.BudgetService.Features.Works.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetById
{
    public class GetWorkByIdHandler : IRequestHandler<GetWorkByIdQuery, GetWorkByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetWorkByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetWorkByIdResponseDto> Handle(GetWorkByIdQuery request, CancellationToken ct)
        {
            var work = await _context.Works
                .Include(w => w.Division)
                .FirstOrDefaultAsync(w => w.WorkCode == request.Code, ct);

            if (work == null)
                throw new KeyNotFoundException($"Work with code '{request.Code}' not found");

            var response = work.Adapt<GetWorkByIdResponseDto>();
            response.DivisionDesc = work.Division?.DivisionDesc;
            response.AccountCode = $"{work.MajorAccCode}-{work.SubMajorAccCode}-{work.MinorAccCode}-{work.SubMinorAccCode}-{work.DetailAccCode}-{work.ObjectAccCode}";

            // Get account description from HeadOfAccount if exists
            var accountDescription = await GetAccountDescription(work.MajorAccCode, work.SubMajorAccCode,
                work.MinorAccCode, work.SubMinorAccCode, work.DetailAccCode, work.ObjectAccCode, ct);
            response.AccountDescription = accountDescription;

            return response;
        }

        private async Task<string?> GetAccountDescription(string majorCode, string subMajorCode,
            string minorCode, string subMinorCode, string detailCode, string objectCode, CancellationToken ct)
        {
            // Query HeadOfAccount table to get full description
            var headOfAccount = await _context.HeadOfAccounts
                .FirstOrDefaultAsync(h =>
                    h.MajorAccCode == majorCode &&
                    h.SubMajorAccCode == subMajorCode &&
                    h.MinorAccCode == minorCode &&
                    h.SubMinorAccCode == subMinorCode &&
                    h.DetailAccCode == detailCode &&
                    h.ObjectAccCode == objectCode, ct);

            return headOfAccount?.HeadDesc;
        }
    }
}
