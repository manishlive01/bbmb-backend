using Npgsql;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// Inserts the minimal office hierarchy the existing masters require (project → CE → circle → Division)
/// with plain SQL, so tests do not depend on master-slice behaviour. Codes are made unique per call.
/// </summary>
public static class TestMasters
{
    private static int _seq = 100;

    public sealed record Hierarchy(string ProjectCode, int CeId, int CeCode, int CrId, int CrCode, int DivisionId, int DivisionCode);

    public static async Task<Hierarchy> InsertDivisionAsync(string connectionString, string userCode = "TEST")
    {
        var n = Interlocked.Increment(ref _seq);
        const string project = "T"; // projects.proj_code is varchar(1): one shared test project per database
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        async Task<int> Scalar(string sql, params object[] args)
        {
            await using var cmd = new NpgsqlCommand(sql, conn);
            for (var i = 0; i < args.Length; i++) cmd.Parameters.AddWithValue($"p{i}", args[i]);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        await Scalar("INSERT INTO budget.projects (proj_code, proj_desc, modification_date, user_code) " +
                     "VALUES (@p0, 'Test project', now(), @p1) ON CONFLICT DO NOTHING RETURNING 1", project, userCode);
        var ceCode = n;
        var ceId = await Scalar("INSERT INTO budget.ce_management (ce_code, name, project_code, status, user_code) " +
                                "VALUES (@p0, 'Test CE', @p1, true, @p2) RETURNING id", ceCode, project, userCode);
        var crCode = n;
        var crId = await Scalar("INSERT INTO budget.cr_management (cr_code, name, status, user_code, ce_id) " +
                                "VALUES (@p0, 'Test Circle', true, @p1, @p2) RETURNING id", crCode, userCode, ceId);
        var divisionCode = n;
        var divisionId = await Scalar("INSERT INTO budget.division (division_code, cr_id, division_desc, modification_date, user_code) " +
                                      "VALUES (@p0, @p1, 'Test Division', now(), @p2) RETURNING id", divisionCode, crId, userCode);
        return new Hierarchy(project, ceId, ceCode, crId, crCode, divisionId, divisionCode);
    }
}
