namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos
{
    /// <summary>
    /// Optional body of <c>POST /api/budget-offices/load-from-hierarchy</c> (design §8.2, implementation-time
    /// change of 2026-10-14). The CE master holds no wing, and DDD T-13 requires one on every CE row
    /// (ck_office_wing), so the wing of each CE is furnished by BBMB at loading (DDD §9 L-03).
    /// </summary>
    public sealed class LoadBudgetOfficesDto
    {
        public List<CeWingDto> CeWings { get; set; } = [];
    }

    /// <summary>The wing (irr_pwr code) of one CE master row.</summary>
    public sealed class CeWingDto
    {
        public int CeId { get; set; }
        public string WingCode { get; set; } = string.Empty;
    }
}
