using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace BBMB.BudgetService.Shared.Services.BudgetActors
{
    /// <summary>
    /// Design §4.3: <c>BeginTransactionAsync</c>, then <b>one</b> statement setting all eleven DDD §7.1 keys
    /// with <c>set_config(key, value, true)</c>. Transaction-local is mandatory under connection pooling
    /// (DV-06). A key without a value is set to <c>''</c>, never skipped, so nothing from an earlier
    /// transaction on the pooled connection can survive.
    /// </summary>
    public sealed class BudgetActionScope(BudgetContext context, IBudgetActorSource actorSource) : IBudgetActionScope
    {
        private const string SetActorSql =
            "SELECT set_config('budget.actor_employee_id', @e, true), " +
            "set_config('budget.actor_charge_id', @c, true), " +
            "set_config('budget.actor_office_id', @o, true), " +
            "set_config('budget.actor_role_code', @r, true), " +
            "set_config('budget.actor_name', @n, true), " +
            "set_config('budget.on_behalf_of_post', @p, true), " +
            "set_config('budget.client_ip', @ip, true), " +
            "set_config('budget.request_id', @q, true), " +
            "set_config('budget.action_code', @a, true), " +
            "set_config('budget.justification', @j, true), " +
            "set_config('budget.no_justification', @nj, true)";

        public async Task<IDbContextTransaction> BeginAsync(
            string actionCode,
            string? justification = null,
            bool noJustification = false,
            string? onBehalfOfPost = null,
            CancellationToken ct = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(actionCode);

            // Handlers resolve the actor and answer M-20 before opening a scope; reaching here without one is
            // a coding defect, and DB-28 would refuse the write anyway.
            var actor = await actorSource.GetAsync(ct)
                ?? throw new InvalidOperationException("No budget actor: the handler must answer M-20 before opening a scope.");

            var tx = await context.Database.BeginTransactionAsync(ct);
            try
            {
                await context.Database.ExecuteSqlRawAsync(SetActorSql,
                [
                    new NpgsqlParameter("e", actor.EmployeeId.ToString()),
                    new NpgsqlParameter("c", actor.ChargeId.ToString()),
                    new NpgsqlParameter("o", actor.OfficeId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    new NpgsqlParameter("r", actor.RoleCode),
                    new NpgsqlParameter("n", actor.ActorName),
                    new NpgsqlParameter("p", onBehalfOfPost ?? actor.OnBehalfOfPost ?? string.Empty),
                    new NpgsqlParameter("ip", actor.ClientIp.ToString()),
                    new NpgsqlParameter("q", actor.RequestId.ToString()),
                    new NpgsqlParameter("a", actionCode),
                    new NpgsqlParameter("j", justification ?? string.Empty),
                    new NpgsqlParameter("nj", noJustification ? "true" : string.Empty),
                ], ct);
                return tx;
            }
            catch
            {
                await tx.DisposeAsync();
                throw;
            }
        }
    }
}
