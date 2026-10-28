namespace BBMB.BudgetService.Features.BudgetProcess.ReadRequests
{
    // Request bodies for the Budget reads moved from GET to POST so that no id or filter travels in the URL
    // (security requirement). One file so that what moved out of the URL can be audited in one place.
    // camelCase JSON via the minimal-API Web defaults. Handlers and query records are unchanged.

    /// <summary>POST /api/account-head-assignments/list.</summary>
    public sealed class HeadAssignmentListRequestDto
    {
        public long? OfficeId { get; set; }
    }

    /// <summary>POST /api/admin-approvals/list and POST /api/technical-sanctions/list.</summary>
    public sealed class WorkFilterRequestDto
    {
        public int? WorkId { get; set; }
    }

    /// <summary>POST /api/budget-cycles/allocations/preview.</summary>
    public sealed class AllocationPreviewRequestDto
    {
        public long CycleId { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    /// <summary>POST /api/budget-cycles/consolidation.</summary>
    public sealed class ConsolidationRequestDto
    {
        public long CycleId { get; set; }
        public string? Category { get; set; }
        public string? Wing { get; set; }
        public string? GroupBy { get; set; }
    }

    /// <summary>POST /api/budget-cycles/distributions.</summary>
    public sealed class DistributionsRequestDto
    {
        public long CycleId { get; set; }
        public string Category { get; set; } = string.Empty;
        public long CeOfficeId { get; set; }
    }

    /// <summary>POST /api/budget-proposals/{detail|amr-predecessor-options|assignable-heads|movements|observations}.</summary>
    public sealed class ProposalReadRequestDto
    {
        public long ProposalId { get; set; }
    }

    /// <summary>POST /api/budget-proposals/demand-lines.</summary>
    public sealed class DemandLinesListRequestDto
    {
        public long ProposalId { get; set; }
        public string? Tab { get; set; }
        public bool IncludeDeleted { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    /// <summary>POST /api/budget-proposals/demand-lines/figure-history.</summary>
    public sealed class FigureHistoryRequestDto
    {
        public long ProposalId { get; set; }
        public long LineId { get; set; }
    }

    /// <summary>POST /api/budget-proposals/demand-lines/detail.</summary>
    public sealed class DemandLineDetailRequestDto
    {
        public long ProposalId { get; set; }
        public long LineId { get; set; }
    }
    /// <summary>POST /api/work-codes/estimate-codes.</summary>
    public sealed class WorkCodeRequestDto
    {
        public string WorkCode { get; set; } = string.Empty;
    }
    /// <summary>POST /api/correction-flags/list.</summary>
    public sealed class CorrectionFlagsListRequestDto
    {
        public long? CycleId { get; set; }
        public bool? Open { get; set; }
    }

    /// <summary>POST /api/reappropriations/detail and POST /api/reappropriations/sources/list.</summary>
    public sealed class ReappropriationDetailRequestDto
    {
        public long ReappropriationId { get; set; }
    }

    /// <summary>POST /api/unforeseen-requests/detail.</summary>
    public sealed class UnforeseenDetailRequestDto
    {
        public long UnforeseenRequestId { get; set; }
    }

    // Pending-gap reads (BE-GAP-01, -03, -04, -05): new POST reads, same conventions.

    /// <summary>POST /api/budget-cycles/allocations/list.</summary>
    public sealed class CycleReadRequestDto
    {
        public long CycleId { get; set; }
    }

    /// <summary>POST /api/unforeseen-authorisations/detail (the V-10 register entryId of an UNFORESEEN_AUTH row).</summary>
    public sealed class UnforeseenAuthorisationDetailRequestDto
    {
        public long UnforeseenAuthorisationId { get; set; }
    }

    /// <summary>POST /api/unforeseen-provisions/list. Both filters optional.</summary>
    public sealed class UnforeseenProvisionsListRequestDto
    {
        public long? CycleId { get; set; }
        public long? OfficeId { get; set; }
    }

    /// <summary>POST /api/audit-trail/list (replaces GET /api/audit-trail?…). Every filter optional.</summary>
    public sealed class AuditTrailListRequestDto
    {
        public long? OfficeId { get; set; }
        public Guid? ActorEmployeeId { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
        public string? TableName { get; set; }
        public string? RecordKey { get; set; }
        /// <summary>BUD-228: only audit rows of records belonging to this cycle (see SearchAuditTrailHandler).</summary>
        public long? CycleId { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
