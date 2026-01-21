namespace BBMB.BudgetService.Features.Designations.GetList.Dtos
{
    public class GetDesignationListRequestDto
    {
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Sorting
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }

        // Filters
        public string? DesignationCode { get; set; }
        public string? DesignationDesc { get; set; }
        public string? UserCode { get; set; }
        public DateTime? ModificationDateFrom { get; set; }
        public DateTime? ModificationDateTo { get; set; }
    }
}
