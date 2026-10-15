namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory.Dtos
{
    /// <summary>
    /// One T-31 version (design §8.3 figure-history; BUD-225 to BUD-227): the level and state it was entered at,
    /// the value, the officer and charge who entered it ([C]), the post acted for, and the justification or the
    /// "continue without justification" choice.
    /// </summary>
    public sealed class FigureHistoryItemDto
    {
        public long FigureVersionId { get; set; }
        /// <summary>RBE or OBE_NEXT.</summary>
        public string Figure { get; set; } = string.Empty;
        public decimal? Value { get; set; }
        public long LevelOfficeId { get; set; }
        public string? LevelOfficeName { get; set; }
        public string StateCode { get; set; } = string.Empty;
        /// <summary>ENTRY, UPLOAD or ALTERATION.</summary>
        public string ChangeSource { get; set; } = string.Empty;
        public string? Justification { get; set; }
        public bool ContinuedWithoutJustification { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string? OnBehalfOfPost { get; set; }
        public Guid OfficerId { get; set; }
        public Guid ChargeId { get; set; }
        public DateTimeOffset EnteredAt { get; set; }
    }
}
