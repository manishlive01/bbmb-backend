namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Update.Dtos
{
    /// <summary>
    /// Design §8.3 <c>PUT …/demand-lines/{lineId}</c>: the Add fields minus lineType and the work, replacing the
    /// line's values (a null figure clears it, BUD-241 part entry).
    /// </summary>
    public sealed class UpdateDemandLineDto
    {
        /// <summary>HEAD: required. WORK: null keeps the work's code. UNFORESEEN: must be null (BUD-277).</summary>
        public long? AccountCodeId { get; set; }
        /// <summary>UNFORESEEN only, required there.</summary>
        public string? UnforeseenName { get; set; }
        public decimal? RbeAmount { get; set; }
        public decimal? ObeNextAmount { get; set; }
        public string? JustificationText { get; set; }
        /// <summary>WORK lines of an AMR work only; null leaves the work's column 14 as it is.</summary>
        public int? PrecedingAmrWorkId { get; set; }
    }
}
