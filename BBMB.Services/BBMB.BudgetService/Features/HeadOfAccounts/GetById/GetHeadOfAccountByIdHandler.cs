using BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById
{
    public class GetHeadOfAccountByIdHandler : IRequestHandler<GetHeadOfAccountByIdQuery, GetHeadOfAccountByIdResponseDto>
    {
        private readonly BudgetContext _context;

        public GetHeadOfAccountByIdHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<GetHeadOfAccountByIdResponseDto> Handle(
    GetHeadOfAccountByIdQuery request,
    CancellationToken ct)
        {
            var headOfAccount = await _context.HeadOfAccounts
                .AsNoTracking()
                .Include(h => h.MajorHead)
                .Include(h => h.MinorHead)
                .Include(h => h.DetailHead)
                .Include(h => h.ObjectClassification)
                .FirstOrDefaultAsync(h =>
                    h.MajorAccCode == request.MajorAccCode &&
                    h.SubMajorAccCode == request.SubMajorAccCode &&
                    h.MinorAccCode == request.MinorAccCode &&
                    h.SubMinorAccCode == request.SubMinorAccCode &&
                    h.DetailAccCode == request.DetailAccCode &&
                    h.ObjectAccCode == request.ObjectAccCode,
                    ct);

            if (headOfAccount == null)
            {
                throw new KeyNotFoundException("Head of account not found");
            }

            var response = headOfAccount.Adapt<GetHeadOfAccountByIdResponseDto>();

            response.MajorHeadDescription =
                headOfAccount.MajorHead?.MajorAccDesc ?? string.Empty;

            response.MinorHeadDescription =
                headOfAccount.MinorHead?.MinorAccDesc ?? string.Empty;

            response.DetailHeadDescription =
                headOfAccount.DetailHead?.DetailAccDesc ?? string.Empty;

            response.ObjectClassificationDescription =
                headOfAccount.ObjectClassification?.ObjectAccDesc ?? string.Empty;

            response.FullAccountCode = headOfAccount.FullAccountCode;
            response.AccountCodeId = headOfAccount.AccountCodeId;
            response.CompleteCode = headOfAccount.CompleteCode;
            response.NotForUse = headOfAccount.NotForUse;
            response.Origin = headOfAccount.Origin;

            return response;
        }

    }
}

