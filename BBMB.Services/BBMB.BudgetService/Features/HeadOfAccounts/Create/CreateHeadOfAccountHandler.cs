using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountHandler : IRequestHandler<CreateHeadOfAccountCommand, Response>
    {
        private readonly BudgetContext _context;

        public CreateHeadOfAccountHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<Response> Handle(CreateHeadOfAccountCommand request, CancellationToken ct)
        {
            // Map DTO to entity
            var headOfAccount = request.dto.Adapt<HeadOfAccount>();
            headOfAccount.ModificationDate = DateTime.UtcNow;

            _context.HeadOfAccounts.Add(headOfAccount);
            await _context.SaveChangesAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Head of account created successfully"
            };
        }
    }
}
