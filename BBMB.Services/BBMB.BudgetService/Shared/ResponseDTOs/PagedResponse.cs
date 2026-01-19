namespace BBMB.BudgetService.Shared.ResponseDTOs
{
    public class PagedResponse<T>
    {
        public string Message { get; set; } = string.Empty;
        public List<T> Data { get; set; } = new();
        public PaginationMetadata Pagination { get; set; } = new();
    }
}
