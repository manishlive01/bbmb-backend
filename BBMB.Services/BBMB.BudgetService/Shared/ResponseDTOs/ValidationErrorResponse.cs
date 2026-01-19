namespace BBMB.BudgetService.Shared.ResponseDTOs
{
    public class ValidationErrorResponse
    {
        public string Error { get; set; } = "validation_error";
        public string Message { get; set; } = "One or more validation errors occurred";
        public List<ValidationError> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
