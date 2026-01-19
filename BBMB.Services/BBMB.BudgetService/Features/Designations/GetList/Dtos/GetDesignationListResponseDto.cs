namespace BBMB.BudgetService.Features.Designations.GetList.Dtos
{
    public class GetDesignationListResponseDto
    {
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationDesc { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public decimal? MinBasic { get; set; }
        public decimal? MaxBasic { get; set; }
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
