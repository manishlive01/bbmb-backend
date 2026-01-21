namespace BBMB.BudgetService.Features.DetailHeads.GetList.Dtos
{
    public class GetDetailHeadListResponseDto
    {
        public string DetailAccCode { get; set; } = string.Empty;
        public string DetailAccDesc { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
    }
}
