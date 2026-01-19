namespace BBMB.BudgetService.Features.Divisions.GetById.Dtos
{
    public class GetDivisionByIdResponseDto
    {
        public int DivisionCode { get; set; }
        public int DivSection { get; set; }
        public string DivisionDesc { get; set; } = string.Empty;
        public int? CtrlOffice1 { get; set; }
        public int? CtrlOffice2 { get; set; }
        public string? RoCode { get; set; }
        public string? MajorAccCode { get; set; }
        public string? AccOffice { get; set; }
        public string? UnitCode { get; set; }
        public string? PoolCode { get; set; }
        public string? UmdUnit { get; set; }
        public string? RegionCode { get; set; }
        public string? DdoTds { get; set; }
        public DateTime ModificationDate { get; set; }
        public int? ParentDivisionCode { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
