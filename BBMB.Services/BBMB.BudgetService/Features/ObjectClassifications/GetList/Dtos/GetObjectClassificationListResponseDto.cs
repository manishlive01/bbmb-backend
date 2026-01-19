namespace BBMB.BudgetService.Features.ObjectClassifications.GetList.Dtos
{
    public class GetObjectClassificationListResponseDto
    {
        public string ObjectAccCode { get; set; } = string.Empty;
        public string ObjectAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
