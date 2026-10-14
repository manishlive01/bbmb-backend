using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountHandler : IRequestHandler<CreateHeadOfAccountCommand, Response>
    {
        private readonly BudgetContext _context;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        public CreateHeadOfAccountHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        {
            _context = context;
            _actorSource = actorSource;
            _scope = scope;
        }
        public async Task<Response> Handle(CreateHeadOfAccountCommand request, CancellationToken ct)
        {
            // budget_head_master carries fn_require_actor from P1 (DB-28).
            if (await _actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            var dto = request.dto;

            bool exists = await _context.HeadOfAccounts.AnyAsync(x =>
                x.MajorAccCode == dto.MajorAccCode &&
                x.SubMajorAccCode == dto.SubMajorAccCode &&
                x.MinorAccCode == dto.MinorAccCode &&
                x.SubMinorAccCode == dto.SubMinorAccCode &&
                x.DetailAccCode == dto.DetailAccCode &&
                x.ObjectAccCode == dto.ObjectAccCode,
                ct
            );

            if (exists)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Head of account already exists"
                };
            }

            var headOfAccount = dto.Adapt<HeadOfAccount>();
            headOfAccount.ModificationDate = DateTime.UtcNow;
            // Design §11.3: LEGACY until P2 rewrites this slice as F-01 (CREATED); the party/employee flag by
            // BUD-267 at creation, not from the request.
            headOfAccount.Origin = HeadOfAccountOrigin.Legacy;
            headOfAccount.PartyEmpRequired = HeadOfAccountOrigin.PartyEmpRequiredFor(headOfAccount.MajorAccCode);
            await using var tx = await _scope.BeginAsync("HEAD_OF_ACCOUNT_CREATE", ct: ct);
            await _context.HeadOfAccounts.AddAsync(headOfAccount, ct);
            await _context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new Response
            {
                IsSucced = true,
                Message = "Head of account created successfully"
            };
        }


    }
}
