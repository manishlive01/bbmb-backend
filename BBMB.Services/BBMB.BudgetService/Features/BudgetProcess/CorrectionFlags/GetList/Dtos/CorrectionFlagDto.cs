namespace BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.GetList.Dtos
{
    /// <summary>
    /// Design §8.6 correction-flags (F-29 support, BUD-309). A flag with the allocated and supported figures,
    /// their difference, and the M-34 text explaining it.
    /// </summary>
    public sealed class CorrectionFlagDto
    {
        public long CorrectionFlagId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int? WorkId { get; set; }
        public long? ProvisionId { get; set; }
        public decimal? FigureAllocated { get; set; }
        public decimal? FigureSupported { get; set; }
        public decimal? Difference { get; set; }
        public bool IsCritical { get; set; }
        public bool Cleared { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
