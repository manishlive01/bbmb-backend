using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Rules;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetWorkCandidates
{
    /// <summary>An AM&amp;R work of the actor's Division without a sanction, pickable in the F-06 AMR-tab "existing work" mode.</summary>
    public sealed record AmrWorkCandidateDto(
        int WorkId, string NameOfWork, long? AccountCodeId, string? CompleteCode, string? HeadDesc, string? WorkCode,
        long? DemandProposalId);
    public sealed record GetAmrWorkCandidatesQuery : IRequest<Response<List<AmrWorkCandidateDto>>>;
    /// <summary>
    /// As <see cref="Create.CreateTechnicalSanctionHandler"/>: the actor must be at a DIVISION office, else M-20. Rows:
    /// AMR-class, non pre-DDD works of that Division with no T-27 sanction (one per AM&amp;R work, BUD-274; M-06).
    /// demandProposalId is a proposal of the current cycle (§5.3: OPEN, greatest current_fin_year) with a non-deleted
    /// line of the work. One query.
    /// </summary>
    public sealed class GetAmrWorkCandidatesHandler(BudgetContext context, IBudgetActorSource actorSource, TechnicalSanctionRules rules)
        : IRequestHandler<GetAmrWorkCandidatesQuery, Response<List<AmrWorkCandidateDto>>>
    {
        public async Task<Response<List<AmrWorkCandidateDto>>> Handle(GetAmrWorkCandidatesQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AmrWorkCandidateDto>>(BudgetMessages.M20);
            if (await rules.ActorDivisionAsync(actor, ct) is not { } division)
                return BudgetResponses.Fail<List<AmrWorkCandidateDto>>(BudgetMessages.M20);
            var divisionId = division.DivisionId;
            var currentCycle = context.BudgetCycles.Where(c => c.Status == CycleStatus.Open)
                .OrderByDescending(c => c.CurrentFinYear).Select(c => (long?)c.CycleId).Take(1);
            var rows = await (
                from w in context.Works.AsNoTracking()
                where w.DivisionId == divisionId && w.WorkClass == WorkClass.Amr && !w.PreDdd
                      && !context.TechnicalSanctions.Any(t => t.WorkId == w.Id)
                from h in context.HeadOfAccounts
                    .Where(h => h.MajorAccCode == w.MajorAccCode && h.SubMajorAccCode == w.SubMajorAccCode &&
                                h.MinorAccCode == w.MinorAccCode && h.SubMinorAccCode == w.SubMinorAccCode &&
                                h.DetailAccCode == w.DetailAccCode && h.ObjectAccCode == w.ObjectAccCode).DefaultIfEmpty()
                orderby w.WorkDesc, w.Id
                select new AmrWorkCandidateDto(
                    w.Id, w.WorkDesc, h != null ? h.AccountCodeId : null, h != null ? h.CompleteCode : null, h != null ? h.HeadDesc : null,
                    w.WorkCode,
                    (from l in context.DemandLines
                     where l.WorkId == w.Id && !l.IsDeleted
                     join p in context.BudgetProposals on l.ProposalId equals p.ProposalId
                     where currentCycle.Contains(p.CycleId)
                     orderby p.ProposalId
                     select (long?)p.ProposalId).FirstOrDefault())).ToListAsync(ct);
            return new Response<List<AmrWorkCandidateDto>> { IsSucced = true, Message = "Work candidates retrieved successfully", Data = rows };
        }
    }
    public sealed class GetAmrWorkCandidatesEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST, empty body; F06.Enter as the create it serves (CreateTechnicalSanctionEndpoint).
            app.MapPost("/api/technical-sanctions/work-candidates",
                    async (IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAmrWorkCandidatesQuery(), ct)))
                .RequireAuthorization("BudgetRight:F06.Enter")
                .WithName("GetAmrWorkCandidates")
                .WithTags("TechnicalSanctions")
                .Produces<Response<List<AmrWorkCandidateDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
