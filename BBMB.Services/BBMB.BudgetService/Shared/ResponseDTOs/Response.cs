namespace BBMB.BudgetService.Shared.ResponseDTOs
{
    public class Response
    {
        public bool IsSucced { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Catalogued message id (SRS §7 "M-01".."M-36", or the interim ids "NOT_FOUND", "ADV-122",
        /// "INTEGRITY"; design §4.9). Null on responses that carry no catalogued message. Additive:
        /// existing callers ignore it.
        /// </summary>
        public string? MessageId { get; set; }
    }
    public class Response<T> : Response
    {
        public T? Data { get; set; }
    }
}
