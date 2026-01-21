namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById.Dtos
{
    public class GetMajorHeadByIdResponseDto
    {
        public string MajorAccCode { get; set; } = string.Empty;
        public string MajorAccDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = "123";
    }
}
