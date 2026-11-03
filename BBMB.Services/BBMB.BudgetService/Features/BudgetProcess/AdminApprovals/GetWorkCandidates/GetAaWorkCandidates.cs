using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.GetWorkCandidates
{
    /// <summary>An AA-class work of the actor's Division without an approval, pickable in the F-03 "existing work" mode.</summary>
    public sealed record AaWorkCandidateDto(
        int WorkId, string NameOfWork, long? AccountCodeId, string? CompleteCode, string? HeadDesc, string? WorkCode,
        long? DemandProposalId);
    public sealed record GetAaWorkCandidatesQuery : IRequest<Response<List<AaWorkCandidateDto>>>;
    /// <summary>
    /// As <see cref="Create.CreateAdminApprovalHandler"/>: the actor must be at a DIVISION office, else M-20. Rows:
    /// AA-class, non pre-DDD works of that Division with no T-25 approval (one per work, M-19). demandProposalId is
    /// a proposal of the current cycle (§5.3: OPEN, greatest current_fin_year) with a non-deleted line of the work.
    /// One query.
    /// </summary>
    public sealed class GetAaWorkCandidatesHandler(BudgetContext context, IBudgetActorSource actorSource, AdminApprovalRules rules)
        : IRequestHandler<GetAaWorkCandidatesQuery, Response<List<AaWorkCandidateDto>>>
    {
        public async Task<Response<List<AaWorkCandidateDto>>> Handle(GetAaWorkCandidatesQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AaWorkCandidateDto>>(BudgetMessages.M20);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division)
                return BudgetResponses.Fail<List<AaWorkCandidateDto>>(BudgetMessages.M20);
            var divisionId = division.DivisionId;
            var currentCycle = context.BudgetCycles.Where(c => c.Status == CycleStatus.Open)
                .OrderByDescending(c => c.CurrentFinYear).Select(c => (long?)c.CycleId).Take(1);
            var rows = await (
                from w in context.Works.AsNoTracking()
                where w.DivisionId == divisionId && w.WorkClass == WorkClass.Aa && !w.PreDdd
                      && !context.AdminApprovals.Any(a => a.WorkId == w.Id)
                from h in context.HeadOfAccounts
                    .Where(h => h.MajorAccCode == w.MajorAccCode && h.SubMajorAccCode == w.SubMajorAccCode &&
                                h.MinorAccCode == w.MinorAccCode && h.SubMinorAccCode == w.SubMinorAccCode &&
                                h.DetailAccCode == w.DetailAccCode && h.ObjectAccCode == w.ObjectAccCode).DefaultIfEmpty()
                orderby w.WorkDesc, w.Id
                select new AaWorkCandidateDto(
                    w.Id, w.WorkDesc, h != null ? h.AccountCodeId : null, h != null ? h.CompleteCode : null, h != null ? h.HeadDesc : null,
                    w.WorkCode,
                    (from l in context.DemandLines
                     where l.WorkId == w.Id && !l.IsDeleted
                     join p in context.BudgetProposals on l.ProposalId equals p.ProposalId
                     where currentCycle.Contains(p.CycleId)
                     orderby p.ProposalId
                     select (long?)p.ProposalId).FirstOrDefault())).ToListAsync(ct);
            return new Response<List<AaWorkCandidateDto>> { IsSucced = true, Message = "Work candidates retrieved successfully", Data = rows };
        }
    }
    public sealed class GetAaWorkCandidatesEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST, empty body; F03.Enter as the create it serves (CreateAdminApprovalEndpoint).
            app.MapPost("/api/admin-approvals/work-candidates",
                    async (IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAaWorkCandidatesQuery(), ct)))
                .RequireAuthorization("BudgetRight:F03.Enter")
                .WithName("GetAaWorkCandidates")
                .WithTags("AdminApprovals")
                .Produces<Response<List<AaWorkCandidateDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
