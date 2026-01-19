namespace BBMB.BudgetService.Features.Classes.GetById.Dtos
{
    public class GetClassByIdResponseDto
    {
        public string ClassCode { get; set; } = string.Empty;
        public string ClassDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
    }
}
