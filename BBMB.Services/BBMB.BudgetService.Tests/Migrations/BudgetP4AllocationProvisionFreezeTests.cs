using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;

namespace BBMB.BudgetService.Tests.Migrations;

/// <summary>
/// P4-06 (design §12 P4 exit, §14 elevated depth; RTM backend rows TC-BUD-BE-*). Database-layer negative and
/// boundary tests for the allocation / provision / freeze / estimate-allocation / unforeseen objects of
/// BudgetP4AllocationProvisionFreeze (DDD §5.7, §5.8, §6; DB-05, DB-19, DB-21, DB-22, DB-23, DB-25, DB-26).
/// These were the deferred test part of P4-05/P4-08/P4-09/P4-11/P4-12; the handlers were built impl-only under
/// the P4 speed rule, so the DB rules they lean on are proven here.
///
/// WHY THE DB LAYER AND NOT THE HANDLER: allocation, freeze and unforeseen authorisation are three of the four
/// areas the steering marks for elevated rigour. The controls are enforced in the database (CONSTRAINT TRIGGERs
/// DEFERRABLE INITIALLY DEFERRED for the at-commit balances, BEFORE INSERT for the preconditions), so a handler
/// test that never commits the forbidden set would pass while the control is absent. Each case therefore drives
/// the trigger directly, under a full DDD §7.1 actor context (DbActor), as the table owner — a refusal here is by
/// trigger, never by privilege.
///
/// Method names embed the BUD/VR/M/S token the row demonstrates (no hyphen, not followed by a digit) so
/// scripts/generate_rtm_bud.py auto-links each to its RTM test-case id. Each case runs on its own fresh database
/// in the shared container (PD-3). Prerequisite rows that are not themselves under test are seeded with
/// session_replication_role = replica (test data only; migrations never disable triggers, §11.1 rule 8), exactly
/// as the P1 paise-guard and the P4 work-code tests do; the row actually under test is always inserted with the
/// triggers live.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP4AllocationProvisionFreezeTests(BudgetDatabaseFixture fixture)
{
    // The migration under test is the final-but-one in the chain; migrating fully (null) applies it and P5.

    private static async Task ExecAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<string?> ScalarAsync(string cs, string sql)
    {
        await using var conn = new NpgsqlConnection(cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? null : v.ToString();
    }

    private static async Task<PostgresException> RefusedAsync(Func<Task> act)
    {
        var ex = (await act.Should().ThrowAsync<PostgresException>()).Which;
        ex.SqlState.Should().Be(PostgresErrorCodes.RaiseException);
        return ex;
    }

    // ---------------------------------------------------------------------------------------------------
    // A fully-migrated database carrying the minimum spine the allocation objects reference: a Division and a
    // CE office, an open financial year, a budget cycle, a head of account, one work, and (where asked) a
    // FULL_BOARD allocation with its minutes attachment. Everything here is seeded with triggers OFF — it is
    // scaffolding, not the row under test.
    // ---------------------------------------------------------------------------------------------------
    private sealed record Spine(
        string Cs, long CeOfficeId, long DivisionOfficeId, long CycleId, int WorkId, long AccountCodeId,
        int FinYear);

    private async Task<Spine> SeedSpineAsync(string name, int finYear = 2026, bool openYear = true)
    {
        var cs = await fixture.CreateFreshDatabaseAsync(name);
        await fixture.MigrateToAsync(cs, null);

        // The master hierarchy (project -> CE -> circle -> Division) and the six account-code segments come from
        // the shared TestMasters helper, so the office/work FKs resolve against real master rows.
        var h = await TestMasters.InsertDivisionAsync(cs);
        var seg = await TestMasters.InsertSegmentsAsync(cs);

        // Financial year, head of account (T-07) and one code-less work, triggers off (scaffolding only; triggers
        // off does NOT bypass CHECK or NOT NULL). work_code is NULL at entry (P4 §4.6).
        await ExecAsync(cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.irr_pwr (irr_pwr_code, irr_pwr_desc, modification_date, user_code) " +
            "VALUES ('I', 'Irrigation', now(), 'T') ON CONFLICT DO NOTHING; " +
            $"INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date{(openYear ? "" : ", closed")}) " +
            $"VALUES ({finYear}, make_date({finYear - 1}, 4, 1), make_date({finYear}, 3, 31){(openYear ? "" : ", now()")}) " +
            "ON CONFLICT (fin_year_closing) DO NOTHING; " +
            // the next year (unclosed) so budget_cycle.next_fin_year FK resolves
            $"INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) " +
            $"VALUES ({finYear + 1}, make_date({finYear}, 4, 1), make_date({finYear + 1}, 3, 31)) " +
            "ON CONFLICT (fin_year_closing) DO NOTHING; " +
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, origin, party_emp_required, modification_date, user_code) " +
            $"VALUES ('{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', " +
            "'Head', 'LEGACY', 'N', now(), 'T') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.work (id, work_code, division_id, work_desc, major_acc_code, sub_major_acc_code, " +
            "minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, " +
            "proposed_budget, remarks, user_code, work_class, entered_via, pre_ddd) " +
            $"VALUES (9001, NULL, {h.DivisionId}, 'Work 9001', '{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', " +
            $"'{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', '', '', '', '', 'T', 'AA', 'LEGACY_OPENING', false); " +
            "SET session_replication_role = origin;");

        // The office and cycle rows carry identity-always PKs AND [C] creation stamps that only fn_require_actor
        // fills, so they are created the real way: under a live actor context (triggers on), acting from the
        // bootstrap IT_CELL office. These rows break no business rule, so a direct insert under context is faithful.
        var itCell = await DbActor.ItCellOfficeIdAsync(cs);
        long ceOfficeId, divisionOfficeId, cycleId;
        await using (var act = await BeginAsync(cs, itCell, "SEED_SPINE"))
        {
            var (conn, tx) = (act.Conn, act.Tx);
            // T-13 CHECKs: a CE carries ce_id + wing_code, a null office_name and a non-null parent; a DIVISION
            // carries division_id and a parent. The CE sits under the bootstrap IT_CELL (any parent satisfies the
            // CHECK; the hierarchy is not under test here). ce_id/division_id FK the TestMasters hierarchy.
            ceOfficeId = await InsertReturningAsync(conn, tx,
                "INSERT INTO budget.office (office_level, ce_id, wing_code, parent_office_id) " +
                $"VALUES ('CE', {h.CeId}, 'I', {itCell}) RETURNING office_id");
            divisionOfficeId = await InsertReturningAsync(conn, tx,
                "INSERT INTO budget.office (office_level, division_id, parent_office_id) " +
                $"VALUES ('DIVISION', {h.DivisionId}, {ceOfficeId}) RETURNING office_id");
            cycleId = await InsertReturningAsync(conn, tx,
                $"INSERT INTO budget.budget_cycle (current_fin_year, next_fin_year, is_first_cycle, status) " +
                $"VALUES ({finYear}, {finYear + 1}, true, 'OPEN') RETURNING cycle_id");
            await tx.CommitAsync();
        }

        var accountCodeId = long.Parse((await ScalarAsync(cs,
            $"SELECT account_code_id FROM budget.budget_head_master WHERE major_acc_code='{seg.Major}' AND minor_acc_code='{seg.Minor}'"))!);

        return new Spine(cs, ceOfficeId, divisionOfficeId, cycleId, 9001, accountCodeId, finYear);
    }

    /// <summary>A connection + transaction carrying the migration officer's actor context, disposed together.
    /// A dedicated type is used because C# does not allow `await using` on a deconstructed tuple.</summary>
    private sealed class ActorTx(NpgsqlConnection conn, NpgsqlTransaction tx) : IAsyncDisposable
    {
        public NpgsqlConnection Conn { get; } = conn;
        public NpgsqlTransaction Tx { get; } = tx;
        public Task CommitAsync() => Tx.CommitAsync();
        public async ValueTask DisposeAsync()
        {
            await Tx.DisposeAsync();
            await Conn.DisposeAsync();
        }
    }

    private static async Task<long> InsertReturningAsync(NpgsqlConnection conn, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    /// <summary>Opens a transaction carrying the migration officer's full actor context at the given office.</summary>
    private async Task<ActorTx> BeginAsync(string cs, long officeId, string action)
    {
        // IncludeErrorDetail so the RAISE … USING DETAIL text (which names the offending condition) is readable;
        // several negatives assert on the DETAIL to tell apart two refusals that share one message code.
        var conn = new NpgsqlConnection(
            new NpgsqlConnectionStringBuilder(cs) { Pooling = false, IncludeErrorDetail = true }.ConnectionString);
        await conn.OpenAsync();
        var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, fixture.MigrationEmployeeId, fixture.MigrationChargeId, officeId,
            role: "MIGRATION", action: action, name: BudgetDatabaseFixture.MigrationEmployeeName);
        return new ActorTx(conn, tx);
    }

    /// <summary>Inserts a live WORK provision for the spine's work and returns its id. Written with triggers
    /// live under the actor context — this is a genuine provision, the holder every balance test draws upon.</summary>
    private async Task<long> InsertWorkProvisionAsync(Spine s, string basis = "RBE", string category = "WORKS")
    {
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "SEED_PROVISION"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.provision (fin_year, basis, category, holder_type, work_id, office_id, account_code_id, " +
            "effective_from, budgetary_check) VALUES (@y, @b, @cat, 'WORK', @w, @o, @a, make_date(@y, 4, 1), true) " +
            "RETURNING provision_id", conn, tx);
        cmd.Parameters.AddWithValue("y", s.FinYear);
        cmd.Parameters.AddWithValue("b", basis);
        cmd.Parameters.AddWithValue("cat", category);
        cmd.Parameters.AddWithValue("w", s.WorkId);
        cmd.Parameters.AddWithValue("o", s.DivisionOfficeId);
        cmd.Parameters.AddWithValue("a", s.AccountCodeId);
        var id = (long)(await cmd.ExecuteScalarAsync())!;
        await tx.CommitAsync();
        return id;
    }

    /// <summary>Seeds a FULL_BOARD allocation (with its minutes attachment) for the cycle and credits the given
    /// provision with an ALLOCATION movement of <paramref name="amount"/>. Allocation and movement are the
    /// legitimate way provision gets its opening balance, so this is seeded with triggers live.</summary>
    private async Task<long> SeedAllocationAndCreditAsync(Spine s, long provisionId, decimal amount, string body = "FULL_BOARD")
    {
        var minutesAttachment = await SeedAttachmentAsync(s, "minutes.pdf"); // the allocation FK requires it

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "ALLOCATE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var alloc = new NpgsqlCommand(
            "INSERT INTO budget.allocation (cycle_id, body, minutes_reference, minutes_date, minutes_attachment_id, " +
            "fa_cao_approval_ref, remarks, lines_activated, total_activated, role_code) " +
            $"VALUES (@c, @body, 'MIN/1', make_date(@y,1,1), {minutesAttachment}, 'FA/1', '', 1, @amt, 'MIGRATION') RETURNING allocation_id",
            conn, tx);
        alloc.Parameters.AddWithValue("c", s.CycleId);
        alloc.Parameters.AddWithValue("body", body);
        alloc.Parameters.AddWithValue("y", s.FinYear);
        alloc.Parameters.AddWithValue("amt", amount);
        var allocationId = (long)(await alloc.ExecuteScalarAsync())!;

        await using var mv = new NpgsqlCommand(
            "INSERT INTO budget.provision_movement (provision_id, movement_type, amount, allocation_id) " +
            "VALUES (@p, 'ALLOCATION', @amt, @a)", conn, tx);
        mv.Parameters.AddWithValue("p", provisionId);
        mv.Parameters.AddWithValue("amt", amount);
        mv.Parameters.AddWithValue("a", allocationId);
        await mv.ExecuteNonQueryAsync();
        await tx.CommitAsync();
        return allocationId;
    }

    // ===================================================================================================
    // DB-05 / BUD-285: a provision is written once; only its supersession marker may change, and once.
    // ===================================================================================================
    [Fact]
    public async Task DB05_BUD285_ProvisionCoreColumnImmutable_WriteOnce()
    {
        var s = await SeedSpineAsync("p4_06_db05_core");
        var provision = await InsertWorkProvisionAsync(s);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "TAMPER"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        // Changing any core column (here effective_from) of a written provision is refused by DB-05.
        await using var upd = new NpgsqlCommand(
            $"UPDATE budget.provision SET effective_from = make_date({s.FinYear}, 5, 1) WHERE provision_id = {provision}", conn, tx);
        var ex = await RefusedAsync(() => upd.ExecuteNonQueryAsync());
        ex.MessageText.Should().Be("DB-05");
    }

    [Fact]
    public async Task DB05_BUD285_SupersessionMarkerSetOnceNeverRepointed()
    {
        var s = await SeedSpineAsync("p4_06_db05_marker");
        var first = await InsertWorkProvisionAsync(s);

        // A superseding RBE provision for the same holder, inserted while marking the first superseded, inside
        // one transaction (the live-unique check is deferred, so this is the only legal way, BUD-285).
        long second;
        await using (var act = await BeginAsync(s.Cs, s.CeOfficeId, "SUPERSEDE"))
        {
            var (conn, tx) = (act.Conn, act.Tx);
            await using var ins = new NpgsqlCommand(
                "INSERT INTO budget.provision (fin_year, basis, category, holder_type, work_id, office_id, " +
                "account_code_id, effective_from, budgetary_check) " +
                $"VALUES ({s.FinYear}, 'RBE', 'WORKS', 'WORK', {s.WorkId}, {s.DivisionOfficeId}, {s.AccountCodeId}, " +
                $"make_date({s.FinYear},4,1), true) RETURNING provision_id", conn, tx);
            second = (long)(await ins.ExecuteScalarAsync())!;
            await using var mark = new NpgsqlCommand(
                $"UPDATE budget.provision SET superseded_by_provision_id = {second} WHERE provision_id = {first}", conn, tx);
            await mark.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        (await ScalarAsync(s.Cs, $"SELECT superseded_by_provision_id FROM budget.provision WHERE provision_id = {first}"))
            .Should().Be(second.ToString());

        // Re-pointing the marker to another provision is refused: it is set once.
        await using var __c2 = await BeginAsync(s.Cs, s.CeOfficeId, "REPOINT"); var (c2, t2) = (__c2.Conn, __c2.Tx);
        await using var repoint = new NpgsqlCommand(
            $"UPDATE budget.provision SET superseded_by_provision_id = {second + 1} WHERE provision_id = {first}", c2, t2);
        // (second + 1 does not exist, but the write-once guard fires before the FK is checked)
        (await RefusedAsync(() => repoint.ExecuteNonQueryAsync())).MessageText.Should().Be("DB-05");
    }

    [Fact]
    public async Task DB05_TwoLiveProvisionsForOneHolderRefusedAtCommit()
    {
        var s = await SeedSpineAsync("p4_06_db05_live");
        await InsertWorkProvisionAsync(s);

        // A second live RBE provision for the same holder key, neither superseded, must be refused at commit.
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "SECOND_LIVE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var ins = new NpgsqlCommand(
            "INSERT INTO budget.provision (fin_year, basis, category, holder_type, work_id, office_id, " +
            "account_code_id, effective_from, budgetary_check) " +
            $"VALUES ({s.FinYear}, 'RBE', 'WORKS', 'WORK', {s.WorkId}, {s.DivisionOfficeId}, {s.AccountCodeId}, " +
            $"make_date({s.FinYear},4,1), true)", conn, tx);
        await ins.ExecuteNonQueryAsync();
        var ex = await RefusedAsync(() => tx.CommitAsync());
        ex.MessageText.Should().Be("DB-05");
    }

    // ===================================================================================================
    // DB-21 / BUD-298: the movements of one unforeseen authorisation sum to nil (balanced UNFORESEEN_OUT/IN).
    // ===================================================================================================
    [Fact]
    public async Task DB21_UnbalancedUnforeseenMovementsRefusedAtCommit()
    {
        var s = await SeedSpineAsync("p4_06_db21_bad");
        var source = await InsertWorkProvisionAsync(s, basis: "RBE");
        await SeedAllocationAndCreditAsync(s, source, 1000);
        var (auth, target) = await SeedUnforeseenAuthorisationShellAsync(s, source);

        // One OUT of 400 from source but an IN of only 300 into target: the set does not balance.
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "UNFORESEEN_MOVE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await InsertUnforeseenMovementAsync(conn, tx, source, -400, auth);
        await InsertUnforeseenMovementAsync(conn, tx, target, 300, auth);
        var ex = await RefusedAsync(() => tx.CommitAsync());
        ex.MessageText.Should().Be("DB-21");
    }

    [Fact]
    public async Task DB21_BalancedUnforeseenMovementsAccepted()
    {
        var s = await SeedSpineAsync("p4_06_db21_good");
        var source = await InsertWorkProvisionAsync(s, basis: "RBE");
        await SeedAllocationAndCreditAsync(s, source, 1000);
        var (auth, target) = await SeedUnforeseenAuthorisationShellAsync(s, source);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "UNFORESEEN_MOVE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await InsertUnforeseenMovementAsync(conn, tx, source, -400, auth);
        await InsertUnforeseenMovementAsync(conn, tx, target, 400, auth);
        await tx.CommitAsync();

        (await ScalarAsync(s.Cs,
                $"SELECT coalesce(sum(amount),0)::bigint FROM budget.provision_movement WHERE unforeseen_authorisation_id = {auth}"))
            .Should().Be("0");
    }

    // ===================================================================================================
    // DB-23 / VR-12 / M-28: Σ estimate allocations of a provision may not exceed the amount available.
    // ===================================================================================================
    [Fact]
    public async Task DB23_VR12_EstimateAllocationAboveProvisionRefusedM28()
    {
        var s = await SeedSpineAsync("p4_06_db23_bad");
        var provision = await InsertWorkProvisionAsync(s);
        await SeedAllocationAndCreditAsync(s, provision, 1000);
        var sanction = await SeedTechnicalSanctionAsync(s);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "ESTIMATE_ALLOC"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await InsertEstimateAllocationAsync(conn, tx, provision, sanction, 1500); // > 1000 available
        var ex = await RefusedAsync(() => tx.CommitAsync());
        ex.MessageText.Should().Be("M-28");
    }

    [Fact]
    public async Task DB23_VR12_EstimateAllocationEqualToProvisionAccepted()
    {
        var s = await SeedSpineAsync("p4_06_db23_boundary");
        var provision = await InsertWorkProvisionAsync(s);
        await SeedAllocationAndCreditAsync(s, provision, 1000);
        var sanction = await SeedTechnicalSanctionAsync(s);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "ESTIMATE_ALLOC"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await InsertEstimateAllocationAsync(conn, tx, provision, sanction, 1000); // exactly the available amount
        await tx.CommitAsync();

        (await ScalarAsync(s.Cs,
                $"SELECT amount::bigint FROM budget.estimate_allocation WHERE provision_id = {provision}"))
            .Should().Be("1000");
    }

    // ===================================================================================================
    // DB-19: once a cycle is frozen, no ALLOCATION movement is accepted (the freeze locks the cycle).
    // ===================================================================================================
    [Fact]
    public async Task DB19_BUD191_FrozenCycleRefusesFurtherAllocationMovement()
    {
        var s = await SeedSpineAsync("p4_06_db19");
        var provision = await InsertWorkProvisionAsync(s);
        var allocationId = await SeedAllocationAndCreditAsync(s, provision, 1000);
        await FreezeCycleAsync(s); // every proposal is at S-15 (there are none) and a FULL_BOARD allocation exists

        (await ScalarAsync(s.Cs, $"SELECT status FROM budget.budget_cycle WHERE cycle_id = {s.CycleId}"))
            .Should().Be("FROZEN");

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "LATE_ALLOCATION"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var mv = new NpgsqlCommand(
            "INSERT INTO budget.provision_movement (provision_id, movement_type, amount, allocation_id) " +
            $"VALUES ({provision}, 'ALLOCATION', 50, {allocationId})", conn, tx);
        (await RefusedAsync(() => mv.ExecuteNonQueryAsync())).MessageText.Should().Be("DB-19");
    }

    [Fact]
    public async Task DB19_BUD190_FreezeWithoutFullBoardAllocationRefused()
    {
        var s = await SeedSpineAsync("p4_06_db19_nofb");
        // No allocation at all: the freeze precondition must refuse.
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "FREEZE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var ins = new NpgsqlCommand(
            $"INSERT INTO budget.freeze (cycle_id, fa_cao_approval_ref, remarks, role_code) " +
            $"VALUES ({s.CycleId}, 'FA/1', '', 'MIGRATION')", conn, tx);
        var ex = await RefusedAsync(() => ins.ExecuteNonQueryAsync());
        ex.MessageText.Should().Be("M-19");
        ex.Detail.Should().Contain("FULL_BOARD");
    }

    // ===================================================================================================
    // DB-22 / BUD-251: no movement is accepted against a provision whose financial year is closed.
    // ===================================================================================================
    [Fact]
    public async Task DB22_BUD251_MovementAgainstClosedYearRefused()
    {
        var s = await SeedSpineAsync("p4_06_db22", openYear: false); // the year is closed in master
        var provision = await InsertWorkProvisionAsync(s);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "LATE_MOVE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        // An UNFORESEEN_IN of 10 against a provision of the closed year: refused before any balance test.
        await using var mv = new NpgsqlCommand(
            "INSERT INTO budget.provision_movement (provision_id, movement_type, amount) " +
            $"VALUES ({provision}, 'UNFORESEEN_IN', 10)", conn, tx);
        var ex = await RefusedAsync(() => mv.ExecuteNonQueryAsync());
        ex.MessageText.Should().Be("DB-22");
    }

    // ===================================================================================================
    // DB-25 / BUD-294 / VR-13: an unforeseen request is only for a work with a recorded admin approval and no
    // work code. A coded work is refused.
    // ===================================================================================================
    [Fact]
    public async Task DB25_BUD294_UnforeseenRequestForCodedWorkRefused()
    {
        var s = await SeedSpineAsync("p4_06_db25_coded");
        await SeedAdminApprovalAsync(s);
        // Give the work a code (triggers off: scaffolding — a coded work is the forbidden precondition). Every
        // T-23 companion must be set to satisfy the pairing CHECKs (a GENERATED code carries origin, occasion,
        // fin_year and at). CHECK constraints are NOT bypassed by session_replication_role, so they must hold.
        await ExecAsync(s.Cs,
            "SET session_replication_role = replica; " +
            $"UPDATE budget.work SET work_code = '740-2526-001', work_code_origin = 'GENERATED', " +
            $"work_code_occasion = 'ALLOC_RBE', work_code_fin_year = {s.FinYear}, work_code_at = now() " +
            $"WHERE id = {s.WorkId}; " +
            "SET session_replication_role = origin;");

        await using var __conn = await BeginAsync(s.Cs, s.DivisionOfficeId, "UNFORESEEN_REQ"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        var ex = await RefusedAsync(() => InsertUnforeseenRequestAsync(conn, tx, s, currentProvisionId: null));
        ex.MessageText.Should().Be("DB-25");
        ex.Detail.Should().Contain("no work code");
    }

    [Fact]
    public async Task DB25_BUD294_UnforeseenRequestWithoutAdminApprovalRefused()
    {
        var s = await SeedSpineAsync("p4_06_db25_noaa");
        // No admin approval for the work: the request must be refused.
        await using var __conn = await BeginAsync(s.Cs, s.DivisionOfficeId, "UNFORESEEN_REQ"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        var ex = await RefusedAsync(() => InsertUnforeseenRequestAsync(conn, tx, s, currentProvisionId: null));
        ex.MessageText.Should().Be("DB-25");
        ex.Detail.Should().Contain("administrative approval");
    }

    // ===================================================================================================
    // DB-26 / VR-13 / M-29: an unforeseen authorisation may not exceed the balance available on the provision
    // drawn upon.
    // ===================================================================================================
    [Fact]
    public async Task DB26_VR13_UnforeseenAuthorisationAboveBalanceRefusedM29()
    {
        var s = await SeedSpineAsync("p4_06_db26_bad");
        var source = await InsertWorkProvisionAsync(s, basis: "RBE");
        await SeedAllocationAndCreditAsync(s, source, 500); // the provision holds only 500
        var request = await SeedUnforeseenRequestRowAsync(s, source);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "UNFORESEEN_AUTH"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        var ex = await RefusedAsync(() => InsertUnforeseenAuthorisationAsync(conn, tx, s, request, amountCurrent: 800));
        ex.MessageText.Should().Be("M-29");
    }

    [Fact]
    public async Task DB26_VR13_UnforeseenAuthorisationWithinBalanceAccepted()
    {
        var s = await SeedSpineAsync("p4_06_db26_good");
        var source = await InsertWorkProvisionAsync(s, basis: "RBE");
        await SeedAllocationAndCreditAsync(s, source, 500);
        var request = await SeedUnforeseenRequestRowAsync(s, source);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "UNFORESEEN_AUTH"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await InsertUnforeseenAuthorisationAsync(conn, tx, s, request, amountCurrent: 400); // within 500
        await tx.CommitAsync();

        (await ScalarAsync(s.Cs,
                $"SELECT amount_current::bigint FROM budget.unforeseen_authorisation WHERE unforeseen_request_id = {request}"))
            .Should().Be("400");
    }

    // ===================================================================================================
    // Audit-trail append-only (Part 11 §11.10(e), Annex 11 §9): the allocation ledger and the freeze record are
    // immutable — a direct UPDATE/DELETE/TRUNCATE is refused even for the table owner.
    // ===================================================================================================
    [Fact]
    public async Task AppendOnly_AllocationAndProvisionMovementAndFreezeCannotBeChangedOrDeleted()
    {
        var s = await SeedSpineAsync("p4_06_append_only");
        var provision = await InsertWorkProvisionAsync(s);
        var allocationId = await SeedAllocationAndCreditAsync(s, provision, 1000);
        await FreezeCycleAsync(s);

        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "TAMPER"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        // Each refusal aborts the transaction, so a SAVEPOINT is taken before each attempt and rolled back to
        // after it, letting the next forbidden statement run in the same live-trigger session.
        async Task Refuse(string sql)
        {
            await using (var sp = new NpgsqlCommand("SAVEPOINT s", conn, tx)) await sp.ExecuteNonQueryAsync();
            (await RefusedAsync(async () =>
            {
                await using var cmd = new NpgsqlCommand(sql, conn, tx);
                await cmd.ExecuteNonQueryAsync();
            })).MessageText.Should().Be("FORBIDDEN");
            await using var rb = new NpgsqlCommand("ROLLBACK TO SAVEPOINT s", conn, tx);
            await rb.ExecuteNonQueryAsync();
        }

        await Refuse($"UPDATE budget.allocation SET remarks = 'x' WHERE allocation_id = {allocationId}");
        await Refuse($"DELETE FROM budget.allocation WHERE allocation_id = {allocationId}");
        await Refuse($"UPDATE budget.provision_movement SET amount = 1 WHERE provision_id = {provision}");
        await Refuse($"DELETE FROM budget.provision_movement WHERE provision_id = {provision}");
        await Refuse($"UPDATE budget.freeze SET remarks = 'x' WHERE cycle_id = {s.CycleId}");
        await Refuse($"DELETE FROM budget.freeze WHERE cycle_id = {s.CycleId}");
    }

    // ---------------------------------------------------------------------------------------------------
    // Shared insert helpers for the rows under test (triggers live).
    // ---------------------------------------------------------------------------------------------------
    private static async Task InsertUnforeseenMovementAsync(NpgsqlConnection conn, NpgsqlTransaction tx,
        long provisionId, decimal amount, long authId)
    {
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.provision_movement (provision_id, movement_type, amount, unforeseen_authorisation_id) " +
            $"VALUES ({provisionId}, @t, @amt, {authId})", conn, tx);
        cmd.Parameters.AddWithValue("t", amount < 0 ? "UNFORESEEN_OUT" : "UNFORESEEN_IN");
        cmd.Parameters.AddWithValue("amt", amount);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task InsertEstimateAllocationAsync(NpgsqlConnection conn, NpgsqlTransaction tx,
        long provisionId, long sanctionId, decimal amount)
    {
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.estimate_allocation (provision_id, technical_sanction_id, amount) " +
            $"VALUES ({provisionId}, {sanctionId}, @amt)", conn, tx);
        cmd.Parameters.AddWithValue("amt", amount);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Attempts the request insert used by the DB-25 negatives. fn_unforeseen_request_rules is a BEFORE
    /// INSERT row trigger, so it refuses before the table CHECKs are evaluated; the amount/provision pair is
    /// supplied anyway so the row would otherwise be well-formed and the refusal is unambiguously DB-25.</summary>
    private static async Task InsertUnforeseenRequestAsync(NpgsqlConnection conn, NpgsqlTransaction tx, Spine s,
        long? currentProvisionId)
    {
        var provisionClause = currentProvisionId is null
            ? "NULL, NULL"
            : $"{currentProvisionId}, 100";
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.unforeseen_request (work_id, division_office_id, current_provision_id, amount_current, " +
            $"next_provision_id, amount_next, status, holder_office_id) " +
            $"VALUES ({s.WorkId}, {s.DivisionOfficeId}, {provisionClause}, NULL, NULL, 'OPEN', {s.DivisionOfficeId})",
            conn, tx);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<long> SeedUnforeseenRequestRowAsync(Spine s, long sourceProvision)
    {
        await SeedAdminApprovalAsync(s);
        await using var __conn = await BeginAsync(s.Cs, s.DivisionOfficeId, "UNFORESEEN_REQ"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        // AT_CE, drawing on the source provision; amount_current pairs with current_provision_id (DDD §5.8 CHECK).
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.unforeseen_request (work_id, division_office_id, current_provision_id, amount_current, " +
            "status, holder_office_id) " +
            $"VALUES ({s.WorkId}, {s.DivisionOfficeId}, {sourceProvision}, 400, 'AT_CE', {s.CeOfficeId}) " +
            "RETURNING unforeseen_request_id",
            conn, tx);
        var id = (long)(await cmd.ExecuteScalarAsync())!;
        await tx.CommitAsync();
        return id;
    }

    /// <summary>Seeds a committed unforeseen request standing AT_CE drawing on the source provision, and returns
    /// (authorisationId, targetProvisionId). The authorisation row itself is NOT inserted — the balance tests
    /// below insert it. The target provision is a fresh live provision the IN movements credit.</summary>
    private async Task<(long AuthId, long TargetProvision)> SeedUnforeseenAuthorisationShellAsync(Spine s, long sourceProvision)
    {
        var request = await SeedUnforeseenRequestRowAsync(s, sourceProvision);
        var auth = await InsertUnforeseenAuthorisationCommittedAsync(s, request, amountCurrent: 400);
        // A second provision (OBE basis, so it is a different holder key) to receive the balancing IN movements.
        long target;
        await using (var act = await BeginAsync(s.Cs, s.CeOfficeId, "SEED_TARGET"))
        {
            var (conn, tx) = (act.Conn, act.Tx);
            await using var ins = new NpgsqlCommand(
                "INSERT INTO budget.provision (fin_year, basis, category, holder_type, work_id, office_id, " +
                "account_code_id, effective_from, budgetary_check) " +
                $"VALUES ({s.FinYear}, 'OBE', 'WORKS', 'WORK', {s.WorkId}, {s.DivisionOfficeId}, {s.AccountCodeId}, " +
                $"make_date({s.FinYear - 1},4,1), true) RETURNING provision_id", conn, tx);
            target = (long)(await ins.ExecuteScalarAsync())!;
            await tx.CommitAsync();
        }
        return (auth, target);
    }

    private async Task<long> InsertUnforeseenAuthorisationCommittedAsync(Spine s, long requestId, decimal amountCurrent)
    {
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "UNFORESEEN_AUTH"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        var id = await InsertUnforeseenAuthorisationAsync(conn, tx, s, requestId, amountCurrent);
        await tx.CommitAsync();
        return id;
    }

    private static async Task<long> InsertUnforeseenAuthorisationAsync(NpgsqlConnection conn, NpgsqlTransaction tx,
        Spine s, long requestId, decimal amountCurrent)
    {
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.unforeseen_authorisation (unforeseen_request_id, ce_office_id, amount_current, reasons, " +
            "request_snapshot, work_code_generated, role_code, client_ip) " +
            $"VALUES ({requestId}, {s.CeOfficeId}, @amt, 'why', '{{}}'::jsonb, '740-2526-001', 'MIGRATION', '192.0.2.1') " +
            "RETURNING unforeseen_authorisation_id", conn, tx);
        cmd.Parameters.AddWithValue("amt", amountCurrent);
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    private async Task<long> SeedTechnicalSanctionAsync(Spine s)
    {
        // A technical sanction for the work with its first version — scaffolding for the estimate_allocation FK,
        // seeded with triggers off (triggers off does NOT bypass CHECKs or NOT NULL, so every required column is
        // supplied). ts_copy_attachment_id is NOT NULL, so a copy attachment is seeded too.
        var copy = await SeedAttachmentAsync(s, "ts.pdf");
        var tsId = long.Parse((await ScalarAsync(s.Cs,
            "SET session_replication_role = replica; " +
            // tab = 'AMR' so no admin_approval_id is required (ck_technical_sanction_approval); estimate_code and
            // estimate_code_at are set together (ck_technical_sanction_estimate_code).
            "INSERT INTO budget.technical_sanction (work_id, tab, serial_no, current_version_no, " +
            "estimate_code, estimate_code_at, created_at, created_by, created_charge_id) " +
            $"VALUES ({s.WorkId}, 'AMR', 1, 1, 'EST/1', now(), now(), '{fixture.MigrationEmployeeId}', '{fixture.MigrationChargeId}') " +
            "RETURNING technical_sanction_id"))!);
        await ExecAsync(s.Cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.technical_sanction_version (technical_sanction_id, version_no, name_of_estimate, " +
            "ts_number, ts_date, ts_amount, ts_copy_attachment_id, created_at, created_by, created_charge_id) " +
            $"VALUES ({tsId}, 1, 'Estimate', 'TS/1', make_date({s.FinYear},1,1), 1000, {copy}, now(), " +
            $"'{fixture.MigrationEmployeeId}', '{fixture.MigrationChargeId}');");
        return tsId;
    }

    private async Task<long> SeedAttachmentAsync(Spine s, string fileName)
    {
        return long.Parse((await ScalarAsync(s.Cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.attachment (storage_key, original_file_name, file_extension, content_type, " +
            "evidence_class, size_bytes, sha256_digest, scan_engine, scanned_at, created_at, created_by, created_charge_id) " +
            $"VALUES (gen_random_uuid(), '{fileName}', 'pdf', 'application/pdf', 'EVIDENCE', 1, " +
            $"repeat('a', 64), 'clamav', now(), now(), '{fixture.MigrationEmployeeId}', '{fixture.MigrationChargeId}') " +
            "RETURNING attachment_id"))!);
    }

    private bool _adminApprovalSeeded;

    private async Task SeedAdminApprovalAsync(Spine s)
    {
        if (_adminApprovalSeeded) return;
        _adminApprovalSeeded = true;
        // Admin approval + version — scaffolding for DB-25's "has a recorded approval" precondition, seeded with
        // triggers off. aa_copy_attachment_id and account_code_id are NOT NULL; account_code_id is the spine's head.
        var copy = await SeedAttachmentAsync(s, "aa.pdf");
        var aaId = long.Parse((await ScalarAsync(s.Cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.admin_approval (work_id, division_office_id, serial_no, " +
            "current_version_no, created_at, created_by, created_charge_id) " +
            $"VALUES ({s.WorkId}, {s.DivisionOfficeId}, 1, 1, now(), '{fixture.MigrationEmployeeId}', '{fixture.MigrationChargeId}') " +
            "RETURNING admin_approval_id"))!);
        await ExecAsync(s.Cs,
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.admin_approval_version (admin_approval_id, version_no, name_of_work, aa_reference_no, " +
            "date_of_approval, account_code_id, amount_sanctioned, aa_copy_attachment_id, competent_authority, " +
            "exceeds_delegation, created_at, created_by, created_charge_id) " +
            $"VALUES ({aaId}, 1, 'Work', 'AA/1', make_date({s.FinYear},1,1), {s.AccountCodeId}, 1000, {copy}, 'CE', false, " +
            $"now(), '{fixture.MigrationEmployeeId}', '{fixture.MigrationChargeId}');");
    }

    private async Task FreezeCycleAsync(Spine s)
    {
        // A FULL_BOARD allocation already exists for the cycle (seeded by the caller); no proposal is open, so the
        // freeze preconditions pass. Inserting the freeze row sets the cycle FROZEN (AFTER-INSERT trigger).
        await using var __conn = await BeginAsync(s.Cs, s.CeOfficeId, "FREEZE"); var (conn, tx) = (__conn.Conn, __conn.Tx);
        await using var ins = new NpgsqlCommand(
            $"INSERT INTO budget.freeze (cycle_id, fa_cao_approval_ref, remarks, role_code) " +
            $"VALUES ({s.CycleId}, 'FA/1', '', 'MIGRATION')", conn, tx);
        await ins.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }
}
