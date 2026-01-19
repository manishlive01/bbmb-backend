namespace BBMB.BudgetService.Features.Projects.GetById.Dtos
{
    public class GetProjectByIdResponseDto
    {
        public string ProjCode { get; set; } = string.Empty;
        public string ProjDesc { get; set; } = string.Empty;
        public DateTime ModificationDate { get; set; }
        public string UserCode { get; set; } = string.Empty;
        public int AssociatedUnitsCount { get; set; }
    }
}
