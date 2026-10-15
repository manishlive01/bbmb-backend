using BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes
{
    /// <summary>
    /// Design §8.8 (F-02, BUD-048 to BUD-050). Actor required (M-20). Reads V-01 (every head as at the query),
    /// stamps <c>producedAt</c>, and marks not-for-use codes. The Manual is not scoped — the chart of accounts is
    /// common — but a reader is required. Ordered by complete code so the layout of R-04 is stable (BUD-050).
    /// </summary>
    public sealed class GetManualOfAccountsCodesHandler(BudgetContext context, IBudgetActorSource actorSource, TimeProvider? clock = null)
        : IRequestHandler<GetManualOfAccountsCodesQuery, Response<ManualOfAccountsResultDto>>
    {
        public async Task<Response<ManualOfAccountsResultDto>> Handle(GetManualOfAccountsCodesQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<ManualOfAccountsResultDto>(BudgetMessages.M20);

            var rows = await context.ManualOfAccountsCodes.AsNoTracking()
                .OrderBy(r => r.CompleteCode)
                .Select(r => new ManualCodeDto(
                    r.AccountCodeId, r.CompleteCode, r.HeadDesc,
                    r.MajorAccCode, r.MajorAccDesc, r.SubMajorAccCode, r.SubMajorAccDesc,
                    r.MinorAccCode, r.MinorAccDesc, r.SubMinorAccCode, r.SubMinorAccDesc,
                    r.DetailAccCode, r.DetailAccDesc, r.ObjectAccCode, r.ObjectAccDesc,
                    r.PartyEmpRequired, r.Origin, r.NotForUse, r.NotForUseRef))
                .ToListAsync(ct);

            return new Response<ManualOfAccountsResultDto>
            {
                IsSucced = true,
                Message = "Manual of Accounts Codes",
                Data = new ManualOfAccountsResultDto((clock ?? TimeProvider.System).GetUtcNow(), rows),
            };
        }
    }
}
