namespace BBMB.BudgetService.Features.BudgetProcess.Offices.LoadFromHierarchy.Dtos
{
    /// <summary>Result of a load (design §8.2). A re-run creates nothing for rows already present.</summary>
    public sealed class LoadBudgetOfficesResponseDto
    {
        /// <summary>T-13 rows created by this run.</summary>
        public int Created { get; set; }
        /// <summary>Offices of the hierarchy already present in T-13 (left unchanged, DDD §9 L-03).</summary>
        public int Existing { get; set; }
        /// <summary>Active CEs not loaded because no wing was supplied for them.</summary>
        public List<UnloadedOfficeDto> CesWithoutWing { get; set; } = [];
        /// <summary>Circles and Divisions not loaded because their parent office is not in T-13.</summary>
        public List<UnloadedOfficeDto> OfficesWithoutParent { get; set; } = [];
    }

    /// <summary>A master row that was not loaded, keyed by its master id.</summary>
    public sealed record UnloadedOfficeDto(string OfficeLevel, int MasterId, string? Name);
}
