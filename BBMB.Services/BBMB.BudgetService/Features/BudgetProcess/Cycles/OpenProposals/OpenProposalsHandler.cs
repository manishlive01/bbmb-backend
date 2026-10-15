using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
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
    /// else M-22 and nothing is created (P2 adds the offices with live T-21 assignments). Only missing proposals
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
            if (headWise.Count > 0 && named.Count > 0)
            {
                var valid = await context.BudgetOffices.AsNoTracking()
                    .Where(o => named.Contains(o.OfficeId) && o.IsActive && HeadWiseLevels.Contains(o.OfficeLevel))
                    .Select(o => o.OfficeId).ToListAsync(ct);
                if (valid.Count != named.Count)
                    return BudgetResponses.Fail<OpenProposalsResponseDto>(BudgetMessages.M22);
                foreach (var c in headWise)
                    expected.AddRange(named.OrderBy(id => id).Select(id => (c, id)));
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
            await tx.CommitAsync(ct);
            result.Created = created.Select(p => new OpenedProposalDto(p.ProposalId, p.Category, p.OriginOfficeId)).ToList();
            return new Response<OpenProposalsResponseDto> { IsSucced = true, Message = "Proposals opened", Data = result };
        }
    }
}
