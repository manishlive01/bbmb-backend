using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Shared.Messages
{
    /// <summary>Builds failed <see cref="Response"/>s that carry a catalogued message (design §4.9).</summary>
    public static class BudgetResponses
    {
        public static Response Fail(string messageId) => new()
        {
            IsSucced = false,
            MessageId = messageId,
            Message = BudgetMessages.Format(messageId),
        };

        public static Response<T> Fail<T>(string messageId, T? data = default) => new()
        {
            IsSucced = false,
            MessageId = messageId,
            Message = BudgetMessages.Format(messageId),
            Data = data,
        };

        /// <summary>A Submit-type refusal listing every failure (BUD-096, BUD-100).</summary>
        public static Response<RuleFailuresDto> RuleFailures(RuleFailuresDto failures)
        {
            ArgumentNullException.ThrowIfNull(failures);
            var first = failures.Failures.FirstOrDefault()?.MessageId
                ?? throw new ArgumentException("At least one failure is required.", nameof(failures));
            return Fail(first, failures);
        }
    }
}
