using BBMB.BudgetService.Features.BudgetProcess.Allocations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Allocations.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Allocations.GetList
{
    /// <summary>
    /// One T-40 allocation of a cycle (BE-GAP-04; F-19 history, BUD-189; the minutes record BUD-279/301 cites).
    /// <see cref="WorkCodesGenerated"/> is not stored on T-40: it is derived from the works of this allocation's
    /// WORK provisions whose code was generated on this allocation's occasion (ALLOC_RBE for BSC, ALLOC_OBE for
    /// FULL_BOARD) for the provision's year, which is what fn_generate_work_code records on the work.
    /// </summary>
    public sealed class AllocationListItemDto
    {
        public long AllocationId { get; set; }
        public long CycleId { get; set; }
        public string Body { get; set; } = string.Empty;
        public string MinutesReference { get; set; } = string.Empty;
        public DateOnly MinutesDate { get; set; }
        public long MinutesAttachmentId { get; set; }
        public string FaCaoApprovalRef { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int LinesActivated { get; set; }
        public decimal TotalActivated { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string? OnBehalfOfPost { get; set; }
        /// <summary>When the allocation was recorded, and by whom (employee id; the name is in the audit trail).</summary>
        public DateTimeOffset AllocatedAt { get; set; }
        public Guid AllocatedBy { get; set; }
        public List<string> WorkCodesGenerated { get; set; } = [];
    }
    public sealed record GetAllocationsQuery(long CycleId) : IRequest<Response<List<AllocationListItemDto>>>;
    /// <summary>
    /// The allocations of one cycle, oldest first. No office scoping: an allocation is made for the whole
    /// cycle by Finance (F19.Allocate, the same right as the preview and the allocate action), not by an office.
    /// Unknown cycle → NOT_FOUND; a cycle with no allocation yet → an empty list.
    /// </summary>
    public sealed class GetAllocationsHandler(BudgetContext context, IBudgetActorSource actorSource)
        : IRequestHandler<GetAllocationsQuery, Response<List<AllocationListItemDto>>>
    {
        public async Task<Response<List<AllocationListItemDto>>> Handle(GetAllocationsQuery request, CancellationToken ct)
        {
            if (await actorSource.GetAsync(ct) is null)
                return BudgetResponses.Fail<List<AllocationListItemDto>>(BudgetMessages.M20);
            if (!await context.BudgetCycles.AsNoTracking().AnyAsync(c => c.CycleId == request.CycleId, ct))
                return BudgetResponses.Fail<List<AllocationListItemDto>>(BudgetMessages.NotFound);
            var rows = await context.Allocations.AsNoTracking()
                .Where(a => a.CycleId == request.CycleId)
                .OrderBy(a => a.AllocationId)
                .ToListAsync(ct);
            var ids = rows.Select(a => a.AllocationId).ToList();
            var coded = await (
                from p in context.Provisions.AsNoTracking()
                where p.AllocationId != null && ids.Contains(p.AllocationId.Value) && p.HolderType == ProvisionHolderType.Work
                join w in context.Works.AsNoTracking() on p.WorkId equals (int?)w.Id
                where w.WorkCode != null && w.WorkCodeFinYear == p.FinYear
                select new { AllocationId = p.AllocationId!.Value, w.WorkCode, w.WorkCodeOccasion })
                .Distinct().ToListAsync(ct);
            var result = rows.Select(a => new AllocationListItemDto
            {
                AllocationId = a.AllocationId,
                CycleId = a.CycleId,
                Body = a.Body,
                MinutesReference = a.MinutesReference,
                MinutesDate = a.MinutesDate,
                MinutesAttachmentId = a.MinutesAttachmentId,
                FaCaoApprovalRef = a.FaCaoApprovalRef,
                Remarks = a.Remarks,
                LinesActivated = a.LinesActivated,
                TotalActivated = a.TotalActivated,
                RoleCode = a.RoleCode,
                OnBehalfOfPost = a.OnBehalfOfPost,
                AllocatedAt = a.CreatedAt,
                AllocatedBy = a.CreatedBy,
                WorkCodesGenerated = coded
                    .Where(c => c.AllocationId == a.AllocationId && c.WorkCodeOccasion == AllocationRules.OccasionFor(a.Body))
                    .Select(c => c.WorkCode!).Distinct().OrderBy(c => c, StringComparer.Ordinal).ToList(),
            }).ToList();
            return new Response<List<AllocationListItemDto>> { IsSucced = true, Message = "Allocations", Data = result };
        }
    }
    public sealed class GetAllocationsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: cycleId travels in the body, not the URL (security requirement).
            app.MapPost("/api/budget-cycles/allocations/list",
                    async (CycleReadRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAllocationsQuery(dto.CycleId), ct)))
                .RequireAuthorization("BudgetRight:F19.Allocate")
                .WithName("GetAllocations")
                .WithTags("Allocations")
                .Produces<Response<List<AllocationListItemDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
