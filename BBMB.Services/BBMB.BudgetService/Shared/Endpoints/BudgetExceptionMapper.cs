using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Shared.Endpoints
{
    /// <summary>Outcome of mapping an exception a database rule raised (design §4.9, §10).</summary>
    public sealed record MappedFailure(int StatusCode, Response Body, string? LogDetail);

    /// <summary>
    /// Maps database-rule failures that reach the global exception handler to the message catalogue:
    /// <list type="bullet">
    /// <item>P0001 with text <c>M-nn</c> → 409 with the catalogue text (a pre-check is missing: logged Warning);</item>
    /// <item>23503 → 409 M-05; 23505 → 409 with the constraint's uniqueness message;</item>
    /// <item><see cref="DbUpdateConcurrencyException"/> (xmin) → 409 M-23.</item>
    /// </list>
    /// Anything else, including P0001 'ACTOR' (DB-28: a handler wrote without opening the scope), is not
    /// mapped and falls through to the existing 500. The body never carries a table, constraint or trigger
    /// name (BUD-012, BUD-261); those go to <see cref="MappedFailure.LogDetail"/> only.
    /// </summary>
    public static class BudgetExceptionMapper
    {
        public static MappedFailure? TryMap(Exception? exception)
        {
            if (exception is null)
                return null;

            if (exception is DbUpdateConcurrencyException)
                return new MappedFailure(StatusCodes.Status409Conflict, BudgetResponses.Fail(BudgetMessages.M23), "xmin concurrency conflict");

            var pg = FindPostgres(exception);
            if (pg is null)
                return null;

            var mapped = BudgetMessages.FromPostgres(pg);
            if (mapped is null)
                return null;

            var detail = $"sqlstate={pg.SqlState} constraint={pg.ConstraintName} table={pg.TableName} where={pg.Where}";
            return new MappedFailure(StatusCodes.Status409Conflict, BudgetResponses.Fail(mapped.Value.MessageId), detail);
        }

        private static PostgresException? FindPostgres(Exception exception)
        {
            for (var e = exception; e is not null; e = e.InnerException)
            {
                if (e is PostgresException pg)
                    return pg;
            }
            return null;
        }
    }
}
