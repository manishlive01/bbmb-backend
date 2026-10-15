using System.Text;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.RunQuery
{
    /// <summary>
    /// Design §8.8 (F-26, BUD-220 to BUD-224). The query runs over <c>budget.v_report_demand</c> only. Every
    /// field — grouped, measured or filtered — is resolved through <see cref="QueryFieldCatalogue"/>; an unknown
    /// business name is refused (M-22) and no client string ever becomes a column or table name. Every filter
    /// value is bound as an Npgsql parameter, so a value containing SQL text is a value, never code. Rows are
    /// scoped to the actor's visible offices (Finance and Secretariat see all) through a bound office-id array.
    /// Any financial year, including a closed one, is queryable (BUD-222). The result is ordered by the grouped
    /// columns so an identical query returns an identical order (BUD-224).
    /// </summary>
    public sealed class RunQueryHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<RunQueryCommand, Response<RunQueryResultDto>>
    {
        public async Task<Response<RunQueryResultDto>> Handle(RunQueryCommand request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<RunQueryResultDto>(BudgetMessages.M20);
            var dto = request.Dto;

            // Resolve group-by dimensions and measures through the whitelist; an unknown field is refused.
            var groupBy = new List<QueryField>();
            foreach (var name in dto.GroupBy)
            {
                var f = QueryFieldCatalogue.Resolve(name);
                if (f is null || f.Kind != QueryFieldKind.Dimension)
                    return BudgetResponses.Fail<RunQueryResultDto>(BudgetMessages.M22);
                groupBy.Add(f);
            }
            var measures = new List<QueryField>();
            foreach (var name in dto.Fields)
            {
                var f = QueryFieldCatalogue.Resolve(name);
                if (f is null || f.Kind != QueryFieldKind.Measure)
                    return BudgetResponses.Fail<RunQueryResultDto>(BudgetMessages.M22);
                measures.Add(f);
            }
            if (measures.Count == 0)
            {
                measures.Add(QueryFieldCatalogue.Resolve("demandedRbe")!);
                measures.Add(QueryFieldCatalogue.Resolve("demandedObeNext")!);
            }

            var parameters = new List<NpgsqlParameter>();
            var where = new StringBuilder();

            // Row scope: Finance and Secretariat see all; others are bound to their visible office ids.
            if (actor.OfficeLevel is not (OfficeLevel.Finance or OfficeLevel.Secretariat))
            {
                var visible = (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToArray();
                where.Append(" AND origin_office_id = ANY(@scope)");
                parameters.Add(new NpgsqlParameter("scope", visible));
            }
            if (dto.FinYear is { } year)
            {
                where.Append(" AND current_fin_year = @finYear");
                parameters.Add(new NpgsqlParameter("finYear", year));
            }
            for (var i = 0; i < dto.Filters.Count; i++)
            {
                var filter = dto.Filters[i];
                var f = QueryFieldCatalogue.Resolve(filter.Field);
                if (f is null || f.Kind != QueryFieldKind.Dimension)
                    return BudgetResponses.Fail<RunQueryResultDto>(BudgetMessages.M22);
                var op = filter.Op?.Trim().ToLowerInvariant() switch { "ne" => "<>", "eq" or null or "" => "=", _ => null };
                if (op is null)
                    return BudgetResponses.Fail<RunQueryResultDto>(BudgetMessages.M22);
                var pName = $"f{i}";
                // The column is a whitelisted identifier; the value is always a bound parameter (never interpolated).
                where.Append($" AND {f.Column}::text {op} @{pName}");
                parameters.Add(new NpgsqlParameter(pName, (object?)filter.Value ?? DBNull.Value));
            }

            var selectCols = new List<string>();
            var columnLabels = new List<string>();
            foreach (var g in groupBy)
            {
                selectCols.Add(g.Column);
                columnLabels.Add(g.Name);
            }
            foreach (var m in measures)
            {
                selectCols.Add($"COALESCE(SUM({m.Column}), 0) AS {m.Column}");
                columnLabels.Add(m.Name);
            }

            var sql = new StringBuilder("SELECT ");
            sql.Append(string.Join(", ", selectCols));
            sql.Append(" FROM budget.v_report_demand WHERE 1=1");
            sql.Append(where);
            if (groupBy.Count > 0)
            {
                sql.Append(" GROUP BY ").Append(string.Join(", ", groupBy.Select(g => g.Column)));
                sql.Append(" ORDER BY ").Append(string.Join(", ", groupBy.Select(g => g.Column)));
            }

            var rows = new List<IReadOnlyList<object?>>();
            var connection = (NpgsqlConnection)context.Database.GetDbConnection();
            var opened = connection.State != System.Data.ConnectionState.Open;
            if (opened)
                await connection.OpenAsync(ct);
            try
            {
                await using var cmd = new NpgsqlCommand(sql.ToString(), connection);
                cmd.Parameters.AddRange(parameters.ToArray());
                await using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    var row = new object?[reader.FieldCount];
                    for (var c = 0; c < reader.FieldCount; c++)
                        row[c] = await reader.IsDBNullAsync(c, ct) ? null : reader.GetValue(c);
                    rows.Add(row);
                }
            }
            finally
            {
                if (opened)
                    await connection.CloseAsync();
            }

            return new Response<RunQueryResultDto>
            {
                IsSucced = true,
                Message = "Query result",
                Data = new RunQueryResultDto(columnLabels, rows),
            };
        }
    }
}
