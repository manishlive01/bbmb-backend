using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetWorkCandidates
{
    /// <summary>POST /api/budget-proposals/work-candidates body.</summary>
    public sealed class DemandWorkCandidatesRequestDto
    {
        public long ProposalId { get; set; }
        /// <summary>AA or AMR (the demand grid tab).</summary>
        public string Tab { get; set; } = string.Empty;
    }
    /// <summary>An existing work the demand grid may line (picked instead of entering a new work).</summary>
    public sealed record DemandWorkCandidateDto(
        int WorkId, string NameOfWork, long? AccountCodeId, string? CompleteCode, string? HeadDesc, string? WorkClass,
        string? WorkCode, long? AdminApprovalId, string? AaReferenceNo, bool HasTechnicalSanction);
    public sealed record GetDemandWorkCandidatesQuery(long ProposalId, string Tab) : IRequest<Response<List<DemandWorkCandidateDto>>>;
    public sealed class GetDemandWorkCandidatesQueryValidator : AbstractValidator<GetDemandWorkCandidatesQuery>
    {
        public GetDemandWorkCandidatesQueryValidator()
        {
            RuleFor(x => x.ProposalId).GreaterThan(0).OverridePropertyName("proposalId").WithMessage("proposalId must be greater than 0.");
            RuleFor(x => x.Tab).Must(t => t is "AA" or "AMR").OverridePropertyName("tab").WithMessage("tab must be AA or AMR.");
        }
    }
    /// <summary>
    /// Works of the proposal's origin Division whose class is the tab's, without a non-deleted line in this
    /// proposal (so VR-14 / M-25 is not provoked by the pick). Name of work is the A&amp;A current version's name
    /// where an approval exists, else work_desc. One query. Out of scope → NOT_FOUND (BUD-031).
    /// </summary>
    public sealed class GetDemandWorkCandidatesHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetDemandWorkCandidatesQuery, Response<List<DemandWorkCandidateDto>>>
    {
        public async Task<Response<List<DemandWorkCandidateDto>>> Handle(GetDemandWorkCandidatesQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<DemandWorkCandidateDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<List<DemandWorkCandidateDto>>(BudgetMessages.NotFound);
            var workClass = request.Tab == "AMR" ? WorkClass.Amr : WorkClass.Aa;
            var (originId, proposalId) = (p.OriginOfficeId, p.ProposalId);
            var rows = await (
                from w in context.Works.AsNoTracking()
                where w.WorkClass == workClass
                      && context.BudgetOffices.Any(o => o.OfficeId == originId && o.DivisionId == w.DivisionId)
                      && !context.DemandLines.Any(l => l.ProposalId == proposalId && l.WorkId == w.Id && !l.IsDeleted)
                from a in context.AdminApprovals.Where(a => a.WorkId == w.Id).DefaultIfEmpty()
                from v in context.AdminApprovalVersions
                    .Where(v => a != null && v.AdminApprovalId == a.AdminApprovalId && v.VersionNo == a.CurrentVersionNo).DefaultIfEmpty()
                from h in context.HeadOfAccounts
                    .Where(h => h.MajorAccCode == w.MajorAccCode && h.SubMajorAccCode == w.SubMajorAccCode &&
                                h.MinorAccCode == w.MinorAccCode && h.SubMinorAccCode == w.SubMinorAccCode &&
                                h.DetailAccCode == w.DetailAccCode && h.ObjectAccCode == w.ObjectAccCode).DefaultIfEmpty()
                let name = v != null ? v.NameOfWork : w.WorkDesc
                orderby name, w.Id
                select new DemandWorkCandidateDto(
                    w.Id, name, h != null ? h.AccountCodeId : null, h != null ? h.CompleteCode : null, h != null ? h.HeadDesc : null,
                    w.WorkClass, w.WorkCode, a != null ? a.AdminApprovalId : null, v != null ? v.AaReferenceNo : null,
                    context.TechnicalSanctions.Any(t => t.WorkId == w.Id))).ToListAsync(ct);
            return new Response<List<DemandWorkCandidateDto>> { IsSucced = true, Message = "Work candidates retrieved successfully", Data = rows };
        }
    }
    public sealed class GetDemandWorkCandidatesEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId travels in the body. F07.Enter: the pick only serves WORKS demand entry.
            app.MapPost("/api/budget-proposals/work-candidates",
                    async (DemandWorkCandidatesRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetDemandWorkCandidatesQuery(dto.ProposalId, dto.Tab ?? string.Empty), ct)))
                .RequireAuthorization("BudgetRight:F07.Enter")
                .WithName("GetDemandWorkCandidates")
                .WithTags("BudgetDemandLines")
                .Produces<Response<List<DemandWorkCandidateDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
