using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Shared.Endpoints
{
    /// <summary>
    /// One status mapping for every budget endpoint (design §2.1, §4.9). Replaces the string matching
    /// on <c>Message</c> the older endpoints use:
    /// M-20 → 403, NOT_FOUND → 404, every other failure → 409 (consistent with the existing Create endpoints).
    /// </summary>
    public static class BudgetResults
    {
        public static IResult From<TResponse>(TResponse response, Func<TResponse, IResult> onSuccess)
            where TResponse : Response
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentNullException.ThrowIfNull(onSuccess);

            if (response.IsSucced)
                return onSuccess(response);

            return response.MessageId switch
            {
                BudgetMessages.M20 => Results.Json(response, statusCode: StatusCodes.Status403Forbidden),
                BudgetMessages.NotFound => Results.NotFound(response),
                _ => Results.Conflict(response),
            };
        }

        /// <summary>Success → 200 with the response body.</summary>
        public static IResult Ok<TResponse>(TResponse response) where TResponse : Response =>
            From(response, r => Results.Ok(r));
    }
}
