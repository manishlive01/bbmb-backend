namespace BBMB.BudgetService.Features.Designations.GetById.Dtos
{
    public class GetDesignationByIdResponseDto
    {
        public string DesignationCode { get; set; } = string.Empty;
        public string DesignationDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
    }
}
