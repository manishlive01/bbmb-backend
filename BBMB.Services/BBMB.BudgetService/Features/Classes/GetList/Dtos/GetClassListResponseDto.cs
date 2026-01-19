namespace BBMB.BudgetService.Features.Classes.GetList.Dtos
{
    public class GetClassListResponseDto
    {
        public string ClassCode { get; set; } = string.Empty;
        public string ClassDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
    }
}
