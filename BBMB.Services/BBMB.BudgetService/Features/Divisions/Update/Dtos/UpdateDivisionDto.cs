namespace BBMB.BudgetService.Features.Divisions.Update.Dtos
{
    public class UpdateDivisionDto
    {
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
        public int? ParentDivisionCode { get; set; }
        public int CRCode { get; set; } 
        public int CECode { get; set; } 
        public string UserCode { get; set; } = "123";
    }
}
