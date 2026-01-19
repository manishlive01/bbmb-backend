using BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode
{
    public class ParseAccountCodeHandler : IRequestHandler<ParseAccountCodeQuery, ParseAccountCodeResponseDto>
    {
        private readonly BudgetContext _context;

        public ParseAccountCodeHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<ParseAccountCodeResponseDto> Handle(ParseAccountCodeQuery request, CancellationToken ct)
        {
            // Account code format: XXXX-XX-XXX-XX-XX-XXXX
            // Example: 2049-01-001-01-01-001
            var parts = request.AccountCode.Split('-');

            if (parts.Length != 6)
                throw new ArgumentException("Invalid account code format. Expected format: XXXX-XX-XXX-XX-XX-XXXX");

            var response = new ParseAccountCodeResponseDto
            {
                MajorAccCode = parts[0],
                SubMajorAccCode = parts[1],
                MinorAccCode = parts[2],
                SubMinorAccCode = parts[3],
                DetailAccCode = parts[4],
                ObjectAccCode = parts[5]
            };

            // Fetch descriptions
            var majorHead = await _context.MajorHeads.FirstOrDefaultAsync(m => m.MajorAccCode == response.MajorAccCode, ct);
            response.MajorHeadDescription = majorHead?.MajorAccDesc;

            var minorHead = await _context.MinorHeads.FirstOrDefaultAsync(m => m.MinorAccCode == response.MinorAccCode, ct);
            response.MinorHeadDescription = minorHead?.MinorAccDesc;

            var detailHead = await _context.DetailHeads.FirstOrDefaultAsync(d => d.DetailAccCode == response.DetailAccCode, ct);
            response.DetailHeadDescription = detailHead?.DetailAccDesc;

            var objectClassification = await _context.ObjectClassifications.FirstOrDefaultAsync(o => o.ObjectAccCode == response.ObjectAccCode, ct);
            response.ObjectClassificationDescription = objectClassification?.ObjectAccDesc;

            return response;
        }
    }
}
