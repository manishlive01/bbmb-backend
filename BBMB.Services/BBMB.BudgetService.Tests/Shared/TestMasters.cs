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

    public sealed record Segments(string Major, string SubMajor, string Minor, string SubMinor, string Detail, string Object)
    {
        public string Complete => $"{Major}-{SubMajor}-{Minor}-{SubMinor}-{Detail}-{Object}";
    }

    /// <summary>
    /// Inserts the six segment masters (T-01 to T-06) of an account code, unique per call. Segment inserts carry
    /// no trigger, so no actor context is needed. Returns the segments; the T-07 row is not inserted.
    /// </summary>
    public static async Task<Segments> InsertSegmentsAsync(string connectionString, string? major = null)
    {
        var n = Interlocked.Increment(ref _seq);
        var seg = new Segments(major ?? (4000 + n % 5000).ToString("D4"), (n % 90 + 10).ToString("D2"),
            (n % 900 + 100).ToString("D3"), (n % 90 + 10).ToString("D2"), (n % 900 + 100).ToString("D3"),
            (n % 900 + 100).ToString("D3"));
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.fa_major_head VALUES (@ma, 'M', now(), 'TEST') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_sub_major_head (major_acc_code, sub_major_acc_code, sub_major_acc_desc, modification_date, user_code) " +
            "VALUES (@ma, @sm, 'SM', now(), 'TEST') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_minor_head VALUES (@mi, 'MI', now(), 'TEST') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_sub_minor_head (sub_minor_acc_code, minor_acc_code, sub_minor_acc_desc, modification_date, user_code) " +
            "VALUES (@sn, @mi, 'SN', now(), 'TEST') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_detail_head (detail_acc_code, detail_acc_desc, modification_date, user_code) " +
            "VALUES (@de, 'DE', now(), 'TEST') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_object_head VALUES (@ob, 'OB', now(), 'TEST') ON CONFLICT DO NOTHING;", conn);
        cmd.Parameters.AddWithValue("ma", seg.Major);
        cmd.Parameters.AddWithValue("sm", seg.SubMajor);
        cmd.Parameters.AddWithValue("mi", seg.Minor);
        cmd.Parameters.AddWithValue("sn", seg.SubMinor);
        cmd.Parameters.AddWithValue("de", seg.Detail);
        cmd.Parameters.AddWithValue("ob", seg.Object);
        await cmd.ExecuteNonQueryAsync();
        return seg;
    }
}
