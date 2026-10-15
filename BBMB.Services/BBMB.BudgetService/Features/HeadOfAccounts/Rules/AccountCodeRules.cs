using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.HeadOfAccounts.Rules
{
    /// <summary>
    /// F-01 rules (design §8.4 head-of-accounts rows; §7 VR-01, BUD-308; DDD DB-03). The database keeps the PKs of
    /// T-01 to T-07, the complete-code UK and trg_head_master_protect as backstops.
    /// </summary>
    public sealed class AccountCodeRules(BudgetContext context)
    {
        /// <summary>Which of the six segments already exist under their parent (major, sub-major under major, …).</summary>
        public sealed record SegmentState(bool Major, bool SubMajor, bool Minor, bool SubMinor, bool Detail, bool Object);

        public async Task<SegmentState> ReadSegmentsAsync(CreateHeadOfAccountDto d, CancellationToken ct = default) => new(
            await context.MajorHeads.AsNoTracking().AnyAsync(x => x.MajorAccCode == d.MajorAccCode, ct),
            await context.SubMajorHeads.AsNoTracking().AnyAsync(x => x.MajorAccCode == d.MajorAccCode && x.SubMajorAccCode == d.SubMajorAccCode, ct),
            await context.MinorHeads.AsNoTracking().AnyAsync(x => x.MinorAccCode == d.MinorAccCode, ct),
            await context.SubMinorHeads.AsNoTracking().AnyAsync(x => x.MinorAccCode == d.MinorAccCode && x.SubMinorAccCode == d.SubMinorAccCode, ct),
            await context.DetailHeads.AsNoTracking().AnyAsync(x => x.DetailAccCode == d.DetailAccCode, ct),
            await context.ObjectClassifications.AsNoTracking().AnyAsync(x => x.ObjectAccCode == d.ObjectAccCode, ct));

        /// <summary>
        /// BUD-322 and VR-01 per level: a code that already exists under its parent is selected, so a description sent
        /// with it is refused M-01 (the code is not new at that level); a code entered new needs its description (M-21).
        /// Every failure is listed (BUD-096 shape).
        /// </summary>
        public static List<RuleFailureItemDto> CheckSegments(CreateHeadOfAccountDto d, SegmentState s)
        {
            var failures = new List<RuleFailureItemDto>();
            void Level(bool exists, string? desc, string field)
            {
                var hasDesc = !string.IsNullOrWhiteSpace(desc);
                if (exists && hasDesc) failures.Add(Item(field, BudgetMessages.M01));
                else if (!exists && !hasDesc) failures.Add(Item(field, BudgetMessages.M21));
            }
            Level(s.Major, d.MajorAccDesc, "majorAccDesc");
            Level(s.SubMajor, d.SubMajorAccDesc, "subMajorAccDesc");
            Level(s.Minor, d.MinorAccDesc, "minorAccDesc");
            Level(s.SubMinor, d.SubMinorAccDesc, "subMinorAccDesc");
            Level(s.Detail, d.DetailAccDesc, "detailAccDesc");
            Level(s.Object, d.ObjectAccDesc, "objectAccDesc");
            return failures;
        }

        /// <summary>VR-01 complete code (M-02): the six segments together already form a code in the Manual.</summary>
        public Task<bool> CompleteCodeExistsAsync(CreateHeadOfAccountDto d, CancellationToken ct = default) =>
            context.HeadOfAccounts.AsNoTracking().AnyAsync(h =>
                h.MajorAccCode == d.MajorAccCode && h.SubMajorAccCode == d.SubMajorAccCode && h.MinorAccCode == d.MinorAccCode &&
                h.SubMinorAccCode == d.SubMinorAccCode && h.DetailAccCode == d.DetailAccCode && h.ObjectAccCode == d.ObjectAccCode, ct);

        /// <summary>
        /// BUD-308 (M-33): a code may be marked not for use only while no demand line refers to it in any cycle. The
        /// allocation (T-41) and re-appropriation (T-46/T-47) arms are added in P4 and P5 with those tables; the
        /// Financial Accounting expenditure arm cannot be checked while that module is unavailable (OI-15).
        /// Returns null when markable, else M-33.
        /// </summary>
        public async Task<string?> CheckMarkableAsync(long accountCodeId, CancellationToken ct = default) =>
            await context.DemandLines.AsNoTracking().AnyAsync(l => l.AccountCodeId == accountCodeId, ct) ? BudgetMessages.M33 : null;

        private static RuleFailureItemDto Item(string field, string messageId) =>
            new(null, null, field, messageId, BudgetMessages.Format(messageId));
    }
}
