using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>
/// P4-02 (design §4.6 steps 1-6, §12 P4 exit, §14 P4 test). The work-code reset is a DESTRUCTIVE data
/// migration, so its preservation proof is written WITH it. These tests seed representative rows BEFORE
/// BudgetP4WorkCodeConformance on a fresh database, apply the migration, and assert that:
///   * every existing work code is preserved in work_code_prev and the live code is nulled (no row lost);
///   * each reset is audited with one T-51 row carrying the migration action code;
///   * every estimate re-links to a work whose preserved code equals the estimate's retained work_code;
///   * an estimate whose work_code matches no work aborts the migration (step-1 guard), listing the row;
///   * the gate unset aborts the migration (§11.1 rule 9);
///   * fn_generate_work_code serialises concurrent allocations for one (Division, year) (DV-03), DB-05
///     refuses a second write of a code, and a generated code matches ^\d{3}-\d{4}-\d{3}$.
/// Each case runs on its own fresh database in the shared container (PD-3).
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP4WorkCodeConformanceTests(BudgetDatabaseFixture fixture)
{
    private const string PreviousMigration = "20261015095625_BudgetP6Reporting";
    private const string ThisMigration = "20261015153531_BudgetP4WorkCodeConformance";

    // The single account code (segments) every seeded work references. A matching budget_head_master row must
    // exist because the six-column work -> budget_head_master FK (BudgetP1ProposalSpineMasters) is re-validated
    // when the migration rewrites work.work_code (int -> varchar).
    private const string SegMajor = "2049", SegSubMajor = "01", SegMinor = "001", SegSubMinor = "00",
        SegDetail = "001", SegObject = "001";

    private static async Task<string?> ScalarAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : v.ToString();
    }

    private static async Task ExecAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Seeds a Division and a financial year through direct inserts (triggers bypassed for seed data only, as
    /// the P1 paise-guard test does; migrations never disable triggers, §11.1 rule 8). Used by the generation
    /// tests, which run after the migration and need a Division code and a financial year for the FK the serial
    /// table and the work-code columns carry.
    /// </summary>
    private static async Task SeedDivisionAndYearAsync(string cs, int divisionId, int divisionCode, int finYearClosing)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            $"INSERT INTO budget.division (id, division_code, cr_id, division_desc, modification_date, user_code) " +
            $"VALUES ({divisionId}, {divisionCode}, 1, 'Div {divisionCode}', now(), 'T') " +
            $"ON CONFLICT (id) DO NOTHING; " +
            $"INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) " +
            $"VALUES ({finYearClosing}, make_date({finYearClosing - 1}, 4, 1), make_date({finYearClosing}, 3, 31)) " +
            $"ON CONFLICT (fin_year_closing) DO NOTHING; " +
            "SET session_replication_role = origin;");
    }

    /// <summary>Inserts just a Division row the work FK (RESTRICT) requires. The migration's rewrite of
    /// work.work_code (int -> varchar) re-validates every FK on the work table, so the Division must exist even
    /// though the seed insert bypasses triggers.</summary>
    private static async Task SeedDivisionAsync(string cs, int divisionId, int divisionCode)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            $"INSERT INTO budget.division (id, division_code, cr_id, division_desc, modification_date, user_code) " +
            $"VALUES ({divisionId}, {divisionCode}, 1, 'Div {divisionCode}', now(), 'T') ON CONFLICT (id) DO NOTHING; " +
            "SET session_replication_role = origin;");
    }

    /// <summary>Inserts the head-of-account row (and its six segment masters) the seeded works reference, so the
    /// six-column work FK holds when the migration re-validates it on the type rewrite. Triggers are bypassed
    /// for the seed only.</summary>
    private static async Task SeedHeadAsync(string cs)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            $"INSERT INTO budget.fa_major_head VALUES ('{SegMajor}', 'M', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_sub_major_head (major_acc_code, sub_major_acc_code, sub_major_acc_desc, modification_date, user_code) VALUES ('{SegMajor}', '{SegSubMajor}', 'SM', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_minor_head VALUES ('{SegMinor}', 'MI', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_sub_minor_head (sub_minor_acc_code, minor_acc_code, sub_minor_acc_desc, modification_date, user_code) VALUES ('{SegSubMinor}', '{SegMinor}', 'SN', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_detail_head (detail_acc_code, detail_acc_desc, modification_date, user_code) VALUES ('{SegDetail}', 'DE', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_object_head VALUES ('{SegObject}', 'OB', now(), 'T') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, origin, party_emp_required, modification_date, user_code) " +
            $"VALUES ('{SegMajor}', '{SegSubMajor}', '{SegMinor}', '{SegSubMinor}', '{SegDetail}', '{SegObject}', 'Head', 'LEGACY', 'N', now(), 'T') ON CONFLICT DO NOTHING; " +
            "SET session_replication_role = origin;");
    }

    /// <summary>Inserts a work BEFORE the reset carrying an integer code (no code companions yet; the T-23
    /// CHECKs land in P4). The Division and the head of account must already exist (seeded separately) for the
    /// FKs the reset re-validates.</summary>
    private static async Task SeedWorkAsync(string cs, int id, int divisionId, int workCode)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.work (id, work_code, division_id, work_desc, major_acc_code, sub_major_acc_code, " +
            "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, " +
            "proposed_budget, remarks, user_code, work_class, entered_via, pre_ddd) " +
            $"VALUES ({id}, {workCode}, {divisionId}, 'Work {id}', '{SegMajor}', '{SegSubMajor}', '{SegMinor}', " +
            $"'{SegSubMinor}', '{SegDetail}', '{SegObject}', '', '', '', '', 'T', 'AA', 'LEGACY_OPENING', false); " +
            "SET session_replication_role = origin;");
    }

    /// <summary>Inserts a code-less work AFTER the reset (work_code null; the T-23 CHECKs require the code
    /// companions to be null too, which a code-less work satisfies). Used by the generation tests.</summary>
    private static async Task SeedCodelessWorkAsync(string cs, int id, int divisionId)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.work (id, work_code, division_id, work_desc, major_acc_code, sub_major_acc_code, " +
            "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, " +
            "proposed_budget, remarks, user_code, work_class, entered_via, pre_ddd) " +
            $"VALUES ({id}, NULL, {divisionId}, 'Work {id}', '{SegMajor}', '{SegSubMajor}', '{SegMinor}', " +
            $"'{SegSubMinor}', '{SegDetail}', '{SegObject}', '', '', '', '', 'T', 'AA', 'LEGACY_OPENING', false); " +
            "SET session_replication_role = origin;");
    }

    private static async Task SeedEstimateAsync(string cs, int workCode, string estimateCode)
    {
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.estimate (work_code, estimate_code, estimate_amt, fin_year_end, division_code) " +
            $"VALUES ({workCode}, '{estimateCode}', 1000, 2026, 1); " +
            "SET session_replication_role = origin;");
    }

    /// <summary>
    /// Opens a transaction carrying the migration officer's full actor context (the eleven DDD §7.1 keys, as
    /// DbActor sets them) and calls fn_generate_work_code. The work-code write it performs fires fn_audit_row,
    /// which needs the whole context (actor_name is NOT NULL on T-51), so every key must be set, not just the ids.
    /// </summary>
    private async Task<string> GenerateCodeAsync(string cs, long officeId, int workId, string occasion, int finYearClosing)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, fixture.MigrationEmployeeId, fixture.MigrationChargeId, officeId,
            role: "MIGRATION", action: "TEST_GEN", name: BudgetDatabaseFixture.MigrationEmployeeName);
        await using var gen = new NpgsqlCommand(
            "SELECT budget.fn_generate_work_code(@w, @o, @y)", conn, tx);
        gen.Parameters.AddWithValue("w", workId);
        gen.Parameters.AddWithValue("o", occasion);
        gen.Parameters.AddWithValue("y", finYearClosing);
        var code = (string?)await gen.ExecuteScalarAsync();
        await tx.CommitAsync();
        return code!;
    }

    [Fact]
    public async Task P4_02_WorkCodesPreservedAndEstimatesRelinked()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("p4_preserve");
        await fixture.MigrateToAsync(cs, PreviousMigration);

        // Three works with distinct integer codes, two estimates on the first, one on the second, none on
        // the third. A representative spread so the preservation and linkage counts are non-trivial.
        await SeedHeadAsync(cs);
        await SeedDivisionAsync(cs, 10, 110);
        await SeedDivisionAsync(cs, 20, 120);
        await SeedWorkAsync(cs, 1, 10, 100001);
        await SeedWorkAsync(cs, 2, 10, 100002);
        await SeedWorkAsync(cs, 3, 20, 200001);
        await SeedEstimateAsync(cs, 100001, "E1");
        await SeedEstimateAsync(cs, 100001, "E2");
        await SeedEstimateAsync(cs, 100002, "E3");

        await fixture.MigrateToAsync(cs, ThisMigration);

        // Preservation: every work keeps its old code in work_code_prev; the live code is nulled.
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.work WHERE work_code_prev IS NOT NULL")).Should().Be("3");
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.work WHERE work_code IS NOT NULL")).Should().Be("0");
        (await ScalarAsync(cs, "SELECT work_code_prev FROM budget.work WHERE id = 1")).Should().Be("100001");
        (await ScalarAsync(cs, "SELECT work_code_prev FROM budget.work WHERE id = 3")).Should().Be("200001");

        // Audit: one T-51 row per work for the work_code reset, carrying the migration action code.
        (await ScalarAsync(cs,
                "SELECT count(*) FROM budget.audit_log WHERE table_name = 'work' AND column_name = 'work_code' " +
                "AND action_code = 'MIGRATION:BudgetP4WorkCodeConformance'"))
            .Should().Be("3");
        // The reset row carried the old code and set it to null.
        (await ScalarAsync(cs,
                "SELECT (old_value #>> '{}')::text FROM budget.audit_log WHERE table_name = 'work' " +
                "AND column_name = 'work_code' AND record_key = '1'"))
            .Should().Be("100001");
        (await ScalarAsync(cs,
                "SELECT new_value IS NULL OR new_value::text = 'null' FROM budget.audit_log WHERE table_name = 'work' " +
                "AND column_name = 'work_code' AND record_key = '1'"))
            .Should().Be("True");

        // Linkage: every estimate keeps its work_code and its work_id resolves to the matching preserved code.
        (await ScalarAsync(cs, "SELECT count(*) FROM budget.estimate WHERE work_id IS NULL")).Should().Be("0");
        (await ScalarAsync(cs,
                "SELECT count(*) FROM budget.estimate e JOIN budget.work w ON w.id = e.work_id " +
                "WHERE w.work_code_prev <> e.work_code"))
            .Should().Be("0");
        (await ScalarAsync(cs,
                "SELECT work_id FROM budget.estimate WHERE work_code = 100002 AND estimate_code = 'E3'"))
            .Should().Be("2");
    }

    [Fact]
    public async Task P4_02_OrphanEstimateAbortsStep1Guard()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("p4_orphan");
        await fixture.MigrateToAsync(cs, PreviousMigration);

        await SeedHeadAsync(cs);
        await SeedDivisionAsync(cs, 10, 110);
        await SeedWorkAsync(cs, 1, 10, 100001);
        await SeedEstimateAsync(cs, 100001, "E1");
        await SeedEstimateAsync(cs, 999999, "E9"); // matches no work

        var act = () => fixture.MigrateToAsync(cs, ThisMigration);
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.MessageText.Should().Contain("unmatched estimate row");
        ex.MessageText.Should().Contain("999999");
        // The migration rolled back: the type change did not happen.
        (await ScalarAsync(cs,
                "SELECT format_type(atttypid, atttypmod) FROM pg_attribute " +
                "WHERE attrelid = 'budget.work'::regclass AND attname = 'work_code'"))
            .Should().Be("integer");
    }

    [Fact]
    public async Task P4_02_GateUnsetAbortsMigration()
    {
        // Fresh database with every gate EXCEPT work_code_reset.
        var gates = BudgetDatabaseFixture.AllGates.Where(g => g != "work_code_reset");
        var cs = await fixture.CreateFreshDatabaseAsync("p4_gate_unset", gates);
        await fixture.MigrateToAsync(cs, PreviousMigration);

        var act = () => fixture.MigrateToAsync(cs, ThisMigration);
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.MessageText.Should().Contain("gate work_code_reset not set");
    }

    [Fact]
    public async Task P4_02_GeneratedCodeFormatAndDB05WriteOnce()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("p4_format");
        await fixture.MigrateToAsync(cs, ThisMigration);
        await SeedHeadAsync(cs);
        await SeedDivisionAndYearAsync(cs, divisionId: 7, divisionCode: 243, finYearClosing: 2026);
        await SeedCodelessWorkAsync(cs, 1, 7);
        var officeId = await DbActor.ItCellOfficeIdAsync(cs);

        var code = await GenerateCodeAsync(cs, officeId, workId: 1, occasion: "ALLOC_RBE", finYearClosing: 2026);
        code.Should().MatchRegex(@"^\d{3}-\d{4}-\d{3}$");
        code.Should().Be("243-2526-001"); // Division 243, year 2026 -> 2526, first serial

        // Idempotent: a second allocation of the same work returns the same code and writes no second code.
        var again = await GenerateCodeAsync(cs, officeId, workId: 1, occasion: "ALLOC_OBE", finYearClosing: 2026);
        again.Should().Be(code);

        // DB-05 write-once: a direct attempt to change the set code is refused (M-06 / P0001). The write runs
        // under the full actor context so fn_require_actor passes and only the write-once trigger refuses it.
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, fixture.MigrationEmployeeId, fixture.MigrationChargeId, officeId,
            role: "MIGRATION", action: "TEST", name: BudgetDatabaseFixture.MigrationEmployeeName);
        await using var upd = new NpgsqlCommand("UPDATE budget.work SET work_code = '243-2526-999' WHERE id = 1", conn, tx);
        var act = () => upd.ExecuteNonQueryAsync();
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.SqlState.Should().Be("P0001");
    }

    [Fact]
    public async Task P4_02_ConcurrentGenerationYieldsDistinctConsecutiveSerials_DV03()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("p4_dv03");
        await fixture.MigrateToAsync(cs, ThisMigration);
        await SeedHeadAsync(cs);
        await SeedDivisionAndYearAsync(cs, divisionId: 7, divisionCode: 243, finYearClosing: 2026);
        await SeedCodelessWorkAsync(cs, 1, 7);
        await SeedCodelessWorkAsync(cs, 2, 7);
        var officeId = await DbActor.ItCellOfficeIdAsync(cs);

        // Two overlapping allocations for the SAME Division and year. The INSERT … ON CONFLICT DO UPDATE …
        // RETURNING on T-24 holds the row lock until commit, so the two cannot compute the same serial.
        var t1 = GenerateCodeAsync(cs, officeId, workId: 1, occasion: "ALLOC_RBE", finYearClosing: 2026);
        var t2 = GenerateCodeAsync(cs, officeId, workId: 2, occasion: "ALLOC_RBE", finYearClosing: 2026);
        var codes = await Task.WhenAll(t1, t2);

        codes.Should().OnlyHaveUniqueItems();
        codes.Should().BeEquivalentTo(new[] { "243-2526-001", "243-2526-002" });
        (await ScalarAsync(cs,
                "SELECT last_serial FROM budget.work_code_serial WHERE division_code = 243 AND fin_year = 2026"))
            .Should().Be("2");
    }
}
