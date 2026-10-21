using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals
{
    /// <summary>
    /// Design §8.2, §6.2 expected set. Cycle absent → NOT_FOUND; not OPEN → M-17. WORKS: every active DIVISION
    /// office. Head-wise (DNA in P1): the named officeIds, each an active DIVISION, CIRCLE or CE office (DB-01),
    /// else M-22 and nothing is created, together with every active office holding a live T-21 assignment (P2, OI-43). Only missing proposals
    /// are created (UK cycle, category, origin), at S-01 held by the origin, in one OPEN_PROPOSALS scope.
    /// </summary>
    public sealed class OpenProposalsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetActionScope scope)
        : IRequestHandler<OpenProposalsCommand, Response<OpenProposalsResponseDto>>
    {
        public const string ActionCode = "OPEN_PROPOSALS";
        private static readonly string[] HeadWiseLevels = [OfficeLevel.Division, OfficeLevel.Circle, OfficeLevel.Ce];

        public async Task<Response<OpenProposalsResponseDto>> Handle(OpenProposalsCommand request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<OpenProposalsResponseDto>(BudgetMessages.M20);

            var status = await context.BudgetCycles.AsNoTracking()
                .Where(c => c.CycleId == request.CycleId).Select(c => c.Status).FirstOrDefaultAsync(ct);
            if (status is null)
                return BudgetResponses.Fail<OpenProposalsResponseDto>(BudgetMessages.NotFound);
            if (status != CycleStatus.Open)
                return BudgetResponses.Fail<OpenProposalsResponseDto>(BudgetMessages.M17);

            var categories = request.Dto.Categories.Distinct().ToList();
            var named = (request.Dto.OfficeIds ?? []).Distinct().ToList();
            var expected = new List<(string Category, long OfficeId)>();

            if (categories.Contains(ProposalCategory.Works))
            {
                var divisions = await context.BudgetOffices.AsNoTracking()
                    .Where(o => o.IsActive && o.OfficeLevel == OfficeLevel.Division)
                    .OrderBy(o => o.OfficeId).Select(o => o.OfficeId).ToListAsync(ct);
                expected.AddRange(divisions.Select(id => (ProposalCategory.Works, id)));
            }
            var headWise = categories.Where(c => c != ProposalCategory.Works).ToList();
            if (headWise.Count > 0)
            {
                if (named.Count > 0)
                {
                    var valid = await context.BudgetOffices.AsNoTracking()
                        .Where(o => named.Contains(o.OfficeId) && o.IsActive && HeadWiseLevels.Contains(o.OfficeLevel))
                        .Select(o => o.OfficeId).ToListAsync(ct);
                    if (valid.Count != named.Count)
                        return BudgetResponses.Fail<OpenProposalsResponseDto>(BudgetMessages.M22);
                }
                // P2: every active office with a live T-21 assignment is expected in each head-wise category opened
                // (OI-43: an assignment carries no category), together with any office named.
                var assigned = await HeadWiseExpectedSet.AssignedOfficesAsync(context, ct);
                var offices = assigned.Union(named).OrderBy(id => id).ToList();
                foreach (var c in headWise)
                    expected.AddRange(offices.Select(id => (c, id)));
            }

            var result = new OpenProposalsResponseDto();
            await using var tx = await scope.BeginAsync(ActionCode, ct: ct);
            // Serialises concurrent opens so the idempotent "only missing proposals" holds (as LoadBudgetOffices).
            await context.Database.ExecuteSqlRawAsync("LOCK TABLE budget.proposal IN SHARE ROW EXCLUSIVE MODE", ct);
            var existing = (await context.BudgetProposals.AsNoTracking()
                    .Where(p => p.CycleId == request.CycleId && categories.Contains(p.Category))
                    .Select(p => new { p.Category, p.OriginOfficeId }).ToListAsync(ct))
                .Select(p => (p.Category, p.OriginOfficeId)).ToHashSet();
            var created = new List<BudgetProposal>();
            foreach (var (category, officeId) in expected)
            {
                if (existing.Contains((category, officeId))) { result.ExistingCount++; continue; }
                var p = new BudgetProposal
                {
                    CycleId = request.CycleId, Category = category, OriginOfficeId = officeId,
                    HolderOfficeId = officeId, StateCode = ProposalStateCode.S01,
                };
                context.BudgetProposals.Add(p);
                created.Add(p);
            }
            await context.SaveChangesAsync(ct);

            // P4 (BUD-080): a newly opened WORKS proposal carries in the continuing works of the Division — the
            // non-AMR works that hold a live operative WORK provision from the previous cycle's approved budget,
            // with their codes. They enter as WORK demand lines without figures (a part entry the Division then
            // completes). AM&R works are not carried in (a new AM&R demand continues the predecessor explicitly).
            await CarryInContinuingWorksAsync(created, ct);

            await context.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            result.Created = created.Select(p => new OpenedProposalDto(p.ProposalId, p.Category, p.OriginOfficeId)).ToList();
            return new Response<OpenProposalsResponseDto> { IsSucced = true, Message = "Proposals opened", Data = result };
        }

        /// <summary>
        /// BUD-080: for each newly opened WORKS proposal, add a WORK demand line for every continuing non-AM&R
        /// work of the proposal's Division — a work with a code and a live operative WORK provision (V-02).
        /// Figures are left null (a part entry); each line is given the next serial. A work already lined in the
        /// proposal is skipped (idempotent with the partial UK on (proposal, work_id)).
        /// </summary>
        private async Task CarryInContinuingWorksAsync(IReadOnlyList<BudgetProposal> created, CancellationToken ct)
        {
            var worksProposals = created.Where(p => p.Category == ProposalCategory.Works).ToList();
            if (worksProposals.Count == 0)
                return;

            // Division id of each WORKS proposal's origin office (works are keyed by division_id).
            var officeIds = worksProposals.Select(p => p.OriginOfficeId).ToList();
            var officeDivision = await context.BudgetOffices.AsNoTracking()
                .Where(o => officeIds.Contains(o.OfficeId) && o.DivisionId != null)
                .Select(o => new { o.OfficeId, DivisionId = o.DivisionId!.Value })
                .ToDictionaryAsync(o => o.OfficeId, o => o.DivisionId, ct);

            var divisionIds = officeDivision.Values.Distinct().ToList();
            if (divisionIds.Count == 0)
                return;

            // Continuing non-AM&R works with a code and a live operative WORK provision (the previous cycle's
            // approved budget), by Division. V-02 is the single source of "a provision is live".
            var continuing = await (
                from w in context.Works.AsNoTracking()
                where w.WorkCode != null && w.WorkClass != Works.Entities.WorkClass.Amr && divisionIds.Contains(w.DivisionId)
                join v in context.ProvisionBalances.AsNoTracking().Where(v => v.IsOperative && v.HolderType == "WORK")
                    on w.Id equals v.WorkId
                select new
                {
                    w.Id, w.DivisionId,
                    w.MajorAccCode, w.SubMajorAccCode, w.MinorAccCode, w.SubMinorAccCode, w.DetailAccCode, w.ObjectAccCode,
                }).Distinct().ToListAsync(ct);
            if (continuing.Count == 0)
                return;

            // Resolve each work's six segments to a T-07 account code for the carried-in line.
            var codeByWork = new Dictionary<int, long?>();
            foreach (var w in continuing)
            {
                var codeId = await context.HeadOfAccounts.AsNoTracking()
                    .Where(h => h.MajorAccCode == w.MajorAccCode && (h.SubMajorAccCode ?? "") == w.SubMajorAccCode &&
                                h.MinorAccCode == w.MinorAccCode && (h.SubMinorAccCode ?? "") == w.SubMinorAccCode &&
                                h.DetailAccCode == w.DetailAccCode && h.ObjectAccCode == w.ObjectAccCode)
                    .Select(h => (long?)h.AccountCodeId).FirstOrDefaultAsync(ct);
                codeByWork[w.Id] = codeId;
            }

            foreach (var proposal in worksProposals)
            {
                if (!officeDivision.TryGetValue(proposal.OriginOfficeId, out var divisionId))
                    continue;
                var works = continuing.Where(w => w.DivisionId == divisionId).Select(w => w.Id).Distinct().ToList();
                if (works.Count == 0)
                    continue;
                var serial = 0;
                foreach (var workId in works.OrderBy(id => id))
                {
                    context.DemandLines.Add(new DemandLine
                    {
                        ProposalId = proposal.ProposalId,
                        LineType = DemandLineType.Work,
                        SerialNo = ++serial,
                        WorkId = workId,
                        AccountCodeId = codeByWork.GetValueOrDefault(workId),
                    });
                }
            }
        }
    }
}
