using BBMB.BudgetService.Features.HeadOfAccounts.Rules;
using BBMB.BudgetService.Features.HeadOfAccounts.Create;
using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>
/// Design §5.2 (T-07 and work orders, six-column FK), §5.5 row a, §11.1 rules 8 and 9, §11.2 (P1 exit test);
/// DDD DV-05, DB-13, DB-28, DB-29.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP1ProposalSpineMastersTests(BudgetDatabaseFixture fixture)
{
    private const string Previous = "20261014140505_BudgetP1ReferenceStateAndAudit";
    private const string Action = "MIGRATION:BudgetP1ProposalSpineMasters";

    private static async Task ExecAsync(string cs, string sql, params (string, object)[] args)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var (n, v) in args) cmd.Parameters.AddWithValue(n, v);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<T?> ScalarAsync<T>(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? default : (T)Convert.ChangeType(v, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    /// <summary>Legacy rows as they stand before the migration: no triggers on T-07 or work yet.</summary>
    private static async Task InsertLegacyHeadAsync(string cs, TestMasters.Segments s, string? partyEmp = null) =>
        await ExecAsync(cs,
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, party_emp_required, modification_date, user_code) " +
            "VALUES (@ma, @sm, @mi, @sn, @de, @ob, 'Legacy head', @pe, now(), 'LEGACY')",
            ("ma", s.Major), ("sm", s.SubMajor), ("mi", s.Minor), ("sn", s.SubMinor), ("de", s.Detail), ("ob", s.Object),
            ("pe", (object?)partyEmp ?? DBNull.Value));

    private static async Task<int> InsertLegacyWorkAsync(string cs, int divisionId, TestMasters.Segments s, int workCode)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.work (work_code, division_id, major_acc_code, sub_major_acc_code, minor_acc_code, " +
            "sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, " +
            "remarks, work_desc, user_code) VALUES (@wc, @d, @ma, @sm, @mi, @sn, @de, @ob, '', '', '', '', 'Legacy work', 'LEGACY') " +
            "RETURNING id", conn);
        cmd.Parameters.AddWithValue("wc", workCode);
        cmd.Parameters.AddWithValue("d", divisionId);
        cmd.Parameters.AddWithValue("ma", s.Major);
        cmd.Parameters.AddWithValue("sm", s.SubMajor);
        cmd.Parameters.AddWithValue("mi", s.Minor);
        cmd.Parameters.AddWithValue("sn", s.SubMinor);
        cmd.Parameters.AddWithValue("de", s.Detail);
        cmd.Parameters.AddWithValue("ob", s.Object);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<(string Cs, TestMasters.Hierarchy H, TestMasters.Segments PartyHead, TestMasters.Segments PlainHead, int[] Works)>
        MigratedWithLegacyDataAsync(string name)
    {
        var cs = await fixture.CreateFreshDatabaseAsync(name);
        await fixture.MigrateToAsync(cs, Previous);
        var h = await TestMasters.InsertDivisionAsync(cs);
        var party = await TestMasters.InsertSegmentsAsync(cs, major: "8443");
        var plain = await TestMasters.InsertSegmentsAsync(cs);
        await InsertLegacyHeadAsync(cs, party);
        await InsertLegacyHeadAsync(cs, plain);
        var w1 = await InsertLegacyWorkAsync(cs, h.DivisionId, plain, 100001);
        var w2 = await InsertLegacyWorkAsync(cs, h.DivisionId, party, 100002);
        await fixture.MigrateToAsync(cs, null);
        return (cs, h, party, plain, [w1, w2]);
    }

    [Fact]
    public async Task BUD036_existing_works_are_marked_pre_ddd_with_an_audited_marker_and_no_invented_creator()
    {
        var (cs, _, _, _, works) = await MigratedWithLegacyDataAsync("spm_works");

        (await ScalarAsync<long>(cs, "SELECT count(*) FROM budget.work WHERE pre_ddd AND created_by IS NULL " +
                                     "AND created_at IS NULL AND created_charge_id IS NULL")).Should().Be(2);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var audit = await ctx.AuditLog.AsNoTracking().Where(a => a.TableName == "work").ToListAsync();
        audit.Where(a => a.ColumnName == "pre_ddd").Select(a => a.RecordKey)
            .Should().BeEquivalentTo(works.Select(w => w.ToString()), "one T-51 row per work for pre_ddd");
        audit.Should().OnlyContain(a => a.ActionCode == Action && a.Operation == "U" && a.ActorRoleCode == "MIGRATION");
        audit.Should().NotContain(a => a.ColumnName != null && a.ColumnName.StartsWith("created_"),
            "the migration never attributes creation");
    }

    [Fact]
    public async Task BUD267_T07_backfills_are_audited_and_derived_columns_are_not()
    {
        var (cs, _, party, plain, _) = await MigratedWithLegacyDataAsync("spm_heads");

        (await ScalarAsync<string>(cs, $"SELECT origin || party_emp_required FROM budget.budget_head_master WHERE major_acc_code = '{party.Major}'"))
            .Should().Be("LEGACYY");
        (await ScalarAsync<string>(cs, $"SELECT origin || party_emp_required FROM budget.budget_head_master WHERE major_acc_code = '{plain.Major}'"))
            .Should().Be("LEGACYN");
        (await ScalarAsync<string>(cs, $"SELECT complete_code FROM budget.budget_head_master WHERE major_acc_code = '{plain.Major}'"))
            .Should().Be(plain.Complete);
        (await ScalarAsync<long>(cs, "SELECT count(DISTINCT account_code_id) FROM budget.budget_head_master")).Should().Be(2);

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var audit = await ctx.AuditLog.AsNoTracking().Where(a => a.TableName == "budget_head_master").ToListAsync();
        audit.Count(a => a.ColumnName == "origin").Should().Be(2);
        audit.Count(a => a.ColumnName == "party_emp_required").Should().Be(2);
        audit.Should().NotContain(a => a.ColumnName == "complete_code" || a.ColumnName == "account_code_id");
        audit.Should().OnlyContain(a => a.ActionCode == Action);
    }

    [Fact]
    public async Task DB28_new_work_in_scope_gets_creation_stamp_which_an_update_cannot_change()
    {
        var (cs, h, _, plain, _) = await MigratedWithLegacyDataAsync("spm_newwork");
        var actor = await TestActor.CreateAsync(fixture, connectionString: cs, name: "Officer W");
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);

        int id;
        await using (var tx = await new BudgetActionScope(ctx, actor).BeginAsync("WORKS_ENTER"))
        {
            var w = new BBMB.BudgetService.Features.Works.Entities.Work
            {
                WorkCode = null, DivisionId = h.DivisionId, WorkDesc = "New", UserCode = "T", // P4 §4.6: code-less at entry
                MajorAccCode = plain.Major, SubMajorAccCode = plain.SubMajor, MinorAccCode = plain.Minor,
                SubMinorAccCode = plain.SubMinor, DetailAccCode = plain.Detail, ObjectAccCode = plain.Object,
                WorkClass = "AA", EnteredVia = "LEGACY_OPENING", PreDdd = false,
            };
            ctx.Works.Add(w);
            await ctx.SaveChangesAsync();
            w.CreatedBy.Should().Be(actor.Actor!.EmployeeId);
            w.CreatedChargeId.Should().Be(actor.Actor.ChargeId);
            w.CreatedAt.Should().NotBeNull();
            id = w.Id;

            await ctx.Database.ExecuteSqlRawAsync(
                $"UPDATE budget.work SET created_by = '{fixture.MigrationEmployeeId}', created_at = '2000-01-01' WHERE id = {id}");
            await tx.CommitAsync();
        }

        (await ScalarAsync<long>(cs, $"SELECT count(*) FROM budget.work WHERE id = {id} AND created_by = '{actor.Actor!.EmployeeId}' AND created_at > '2001-01-01'"))
            .Should().Be(1);
    }

    [Fact]
    public async Task SPM_pre_ddd_is_one_way_and_work_and_segments_refuse_delete_and_truncate()
    {
        var (cs, h, _, plain, works) = await MigratedWithLegacyDataAsync("spm_oneway");
        var actor = await TestActor.CreateAsync(fixture, connectionString: cs);
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();

        async Task<PostgresException> Refused(string sql)
        {
            await using var tx = await conn.BeginTransactionAsync();
            await DbActor.SetAsync(conn, tx, actor.Actor!.EmployeeId, actor.Actor.ChargeId, actor.Actor.OfficeId);
            await using var cmd = new NpgsqlCommand(sql, conn, tx);
            var act = () => cmd.ExecuteNonQueryAsync();
            return (await act.Should().ThrowAsync<PostgresException>()).Which;
        }

        (await Refused(
            "INSERT INTO budget.work (work_code, division_id, major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, remarks, work_desc, user_code, pre_ddd) " +
            $"VALUES (300001, {h.DivisionId}, '{plain.Major}', '{plain.SubMajor}', '{plain.Minor}', '{plain.SubMinor}', '{plain.Detail}', " +
            $"'{plain.Object}', '', '', '', '', 'x', 'T', true)")).MessageText.Should().Be("PRE_DDD_ONE_WAY");

        // Clear the marker on one legacy work (permitted direction), then try to set it again.
        await using (var tx = await conn.BeginTransactionAsync())
        {
            await DbActor.SetAsync(conn, tx, actor.Actor!.EmployeeId, actor.Actor.ChargeId, actor.Actor.OfficeId);
            await using var cmd = new NpgsqlCommand(
                $"UPDATE budget.work SET pre_ddd = false, work_class = 'AA', entered_via = 'LEGACY_OPENING' WHERE id = {works[0]}", conn, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        (await Refused($"UPDATE budget.work SET pre_ddd = true WHERE id = {works[0]}")).MessageText.Should().Be("PRE_DDD_ONE_WAY");

        // DV-05 / DB-13: as the table owner.
        (await Refused($"DELETE FROM budget.work WHERE id = {works[1]}")).MessageText.Should().Be("FORBIDDEN");
        (await Refused("TRUNCATE budget.fa_major_head CASCADE")).MessageText.Should().Be("FORBIDDEN");
        (await Refused("DELETE FROM budget.budget_head_master")).MessageText.Should().Be("FORBIDDEN");
        (await ScalarAsync<long>(cs, "SELECT count(*) FROM budget.work")).Should().Be(2);
    }

    [Fact]
    public async Task OI41_work_whose_segments_match_no_account_code_aborts_with_the_work_listed()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("spm_unmatched");
        await fixture.MigrateToAsync(cs, Previous);
        var h = await TestMasters.InsertDivisionAsync(cs);
        var seg = await TestMasters.InsertSegmentsAsync(cs);
        var id = await InsertLegacyWorkAsync(cs, h.DivisionId, seg, 400001); // no T-07 row for these segments

        var act = () => fixture.MigrateToAsync(cs, null);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().Contain("works whose six segments match no account code").And.Contain($"work {id} ({seg.Complete})");
        (await ScalarAsync<string>(cs, "SELECT to_regproc('budget.fn_work_pre_ddd_one_way')::text")).Should().BeNull();
        (await ScalarAsync<long>(cs, "SELECT count(*) FROM information_schema.columns WHERE table_schema = 'budget' " +
                                     "AND table_name = 'work' AND column_name = 'pre_ddd'")).Should().Be(0, "rolled back");
    }

    [Fact]
    public async Task SPM_sub_major_longer_than_two_aborts_with_the_row_listed()
    {
        var cs = await fixture.CreateFreshDatabaseAsync("spm_width");
        await fixture.MigrateToAsync(cs, Previous);
        var seg = await TestMasters.InsertSegmentsAsync(cs);
        // T-07 has no FK to T-02 yet, so a three-character sub-major can stand in the legacy table.
        await InsertLegacyHeadAsync(cs, seg with { SubMajor = "123" });

        var act = () => fixture.MigrateToAsync(cs, null);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().Contain("sub_major_acc_code longer than 2").And.Contain((seg with { SubMajor = "123" }).Complete);
    }

    [Fact]
    public async Task Rule9_gate_unset_aborts_the_migration()
    {
        var gates = BudgetDatabaseFixture.AllGates.Where(g => g != "p1_ddd_deviations");
        var cs = await fixture.CreateFreshDatabaseAsync("spm_nogate", gates);
        var act = () => fixture.MigrateToAsync(cs, null);
        (await act.Should().ThrowAsync<PostgresException>()).Which.MessageText
            .Should().StartWith("gate p1_ddd_deviations not set");
        (await ScalarAsync<string>(cs, "SELECT to_regclass('budget.audit_log')::text"))
            .Should().Be("budget.audit_log", "the ungated migrations before it were applied");
    }

    [Fact]
    public async Task BUD267_HeadOfAccount_create_through_the_handler_inside_a_scope_succeeds()
    {
        // P2-10 rewrote the slice as F-01 (origin CREATED, e-Office reference required); the P1 property under test,
        // that the T-07 insert runs inside the action scope with the actor and BUD-267 flag, still holds.
        var seg = await TestMasters.InsertSegmentsAsync(fixture.PlainConnectionString, major: "9906");
        var actor = await TestActor.CreateAsync(fixture, name: "Officer H");
        await using var ctx = fixture.NewBudgetContext();
        var handler = new CreateHeadOfAccountHandler(ctx, actor, new BudgetActionScope(ctx, actor), new AccountCodeRules(ctx));
        var r = await handler.Handle(new CreateHeadOfAccountCommand(new CreateHeadOfAccountDto
        {
            MajorAccCode = seg.Major, SubMajorAccCode = seg.SubMajor, MinorAccCode = seg.Minor,
            SubMinorAccCode = seg.SubMinor, DetailAccCode = seg.Detail, ObjectAccCode = seg.Object,
            HeadDesc = "Created head", EofficeApprovalRef = "EO/2026/1",
        }), default);
        r.IsSucced.Should().BeTrue(r.Message);
        await using var read = fixture.NewBudgetContext();
        var head = await read.HeadOfAccounts.AsNoTracking().SingleAsync(x => x.MajorAccCode == seg.Major && x.ObjectAccCode == seg.Object && x.MinorAccCode == seg.Minor);
        head.Origin.Should().Be("CREATED");
        head.PartyEmpRequired.Should().Be("Y", "BUD-267 sets the flag for major 9906; the request cannot carry it");
        head.CreatedBy.Should().Be(actor.Actor!.EmployeeId);
        head.CompleteCode.Should().Be(seg.Complete);
        (await read.AuditLog.CountAsync(a => a.TableName == "budget_head_master" && a.Operation == "I"
                                             && a.ActionCode == CreateHeadOfAccountHandler.ActionCode)).Should().BeGreaterThan(0);
    }
    [Fact]
    public async Task M20_HeadOfAccount_create_without_an_actor_is_refused()
    {
        await using var ctx = fixture.NewBudgetContext();
        var none = FakeBudgetActorSource.None();
        var r = await new CreateHeadOfAccountHandler(ctx, none, new BudgetActionScope(ctx, none), new AccountCodeRules(ctx))
            .Handle(new CreateHeadOfAccountCommand(new CreateHeadOfAccountDto { MajorAccCode = "0000" }), default);
        r.MessageId.Should().Be("M-20");
    }
}
