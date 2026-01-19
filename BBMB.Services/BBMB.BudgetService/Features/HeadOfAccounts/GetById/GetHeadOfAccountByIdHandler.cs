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

        public async Task<GetHeadOfAccountByIdResponseDto> Handle(GetHeadOfAccountByIdQuery request, CancellationToken ct)
        {
            var headOfAccount = await _context.HeadOfAccounts
                .Include(h => h.MajorHead)
                .Include(h => h.MinorHead)
                .Include(h => h.DetailHead)
                .Include(h => h.ObjectClassification)
                .FirstOrDefaultAsync(h => h.Id == request.Id, ct);

            if (headOfAccount == null)
            {
                throw new KeyNotFoundException($"Head of account with ID '{request.Id}' not found");
            }

            var response = headOfAccount.Adapt<GetHeadOfAccountByIdResponseDto>();
            response.MajorHeadDescription = headOfAccount.MajorHead?.MajorAccDesc ?? string.Empty;
            response.MinorHeadDescription = headOfAccount.MinorHead?.MinorAccDesc ?? string.Empty;
            response.DetailHeadDescription = headOfAccount.DetailHead?.DetailAccDesc ?? string.Empty;
            response.ObjectClassificationDescription = headOfAccount.ObjectClassification?.ObjectAccDesc ?? string.Empty;
            response.FullAccountCode = headOfAccount.FullAccountCode;

            return response;
        }
    }
}
