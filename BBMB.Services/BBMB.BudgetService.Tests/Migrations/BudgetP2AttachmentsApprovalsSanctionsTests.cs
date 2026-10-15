using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Npgsql;
namespace BBMB.BudgetService.Tests.Migrations;
/// <summary>
/// Database-layer tests for BudgetP2AttachmentsApprovalsSanctions (DDD DB-04 to DB-09, DB-27; DV-12 to
/// DV-15; design §5.3 P2, §11.2). The connection is the table owner, so every refusal is by trigger or CHECK.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class BudgetP2AttachmentsApprovalsSanctionsTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"p2aas_{Guid.NewGuid():N}"[..22]);
    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task<long> ScalarAsync(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        var v = await cmd.ExecuteScalarAsync();
        return v is null or DBNull ? 0 : Convert.ToInt64(v);
    }
    private static async Task ExecAsync(NpgsqlConnection c, NpgsqlTransaction tx, string sql)
    {
        await using var cmd = new NpgsqlCommand(sql, c, tx);
        await cmd.ExecuteNonQueryAsync();
    }
    private static async Task<PostgresException> RefusedAsync(Func<Task> act) =>
        (await act.Should().ThrowAsync<PostgresException>()).Which;

    private static string Attachment(string evidenceClass = "EVIDENCE", string ext = "pdf") =>
        "INSERT INTO budget.attachment (storage_key, original_file_name, file_extension, content_type, evidence_class, " +
        "size_bytes, sha256_digest, scan_engine, scanned_at) VALUES (gen_random_uuid(), 'copy." + ext + "', '" + ext + "', " +
        "'application/pdf', '" + evidenceClass + "', 1024, repeat('a', 64), 'TEST', now()) RETURNING attachment_id";

    private string ApprovalHeader(int workId, int serial) =>
        "INSERT INTO budget.admin_approval (work_id, division_office_id, serial_no, current_version_no) " +
        $"VALUES ({workId}, {_w.DivisionAOfficeId}, {serial}, 1) RETURNING admin_approval_id";

    private string ApprovalVersion(long approvalId, int versionNo, long attachmentId, decimal amount, string reference = "AA/1") =>
        "INSERT INTO budget.admin_approval_version (admin_approval_id, version_no, name_of_work, aa_reference_no, date_of_approval, " +
        "account_code_id, amount_sanctioned, aa_copy_attachment_id, competent_authority, exceeds_delegation) " +
        $"VALUES ({approvalId}, {versionNo}, 'Work', '{reference}', current_date - 1, {_w.AccountCodeId}, {amount}, {attachmentId}, 'CE', false)";

    private static string SanctionHeader(int workId, string tab, long? approvalId, int serial) =>
        "INSERT INTO budget.technical_sanction (work_id, tab, admin_approval_id, serial_no, current_version_no) " +
        $"VALUES ({workId}, '{tab}', {(approvalId is null ? "NULL" : approvalId.ToString())}, {serial}, 1) RETURNING technical_sanction_id";

    private static string SanctionVersion(long sanctionId, int versionNo, long attachmentId, decimal amount, string tsNumber) =>
        "INSERT INTO budget.technical_sanction_version (technical_sanction_id, version_no, name_of_estimate, ts_number, ts_date, " +
        $"ts_amount, ts_copy_attachment_id) VALUES ({sanctionId}, {versionNo}, 'Estimate', '{tsNumber}', current_date - 1, {amount}, {attachmentId})";

    /// <summary>Commits an approval of <paramref name="amount"/> on work A (version 1) and returns its id.</summary>
    private async Task<long> CommittedApprovalAsync(decimal amount, int workId = 0, int serial = 1, string reference = "AA/1")
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_CREATE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        var id = await ScalarAsync(c, tx, ApprovalHeader(workId == 0 ? _w.WorkAId : workId, serial));
        await ExecAsync(c, tx, ApprovalVersion(id, 1, att, amount, reference));
        await tx.CommitAsync();
        return id;
    }

    /// <summary>Records an AA-tab sanction of <paramref name="amount"/>; the transaction is returned uncommitted.</summary>
    private async Task<(NpgsqlConnection, NpgsqlTransaction)> SanctionAsync(long approvalId, decimal amount, int serial, string tsNumber)
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        var att = await ScalarAsync(c, tx, Attachment());
        var ts = await ScalarAsync(c, tx, SanctionHeader(_w.WorkAId, "AA", approvalId, serial));
        await ExecAsync(c, tx, SanctionVersion(ts, 1, att, amount, tsNumber));
        return (c, tx);
    }

    private async Task<int> NewWorkAsync(string workClass, int divisionId, FakeBudgetActorSource actor)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, actor).BeginAsync("WORKS_ENTER");
        var seg = _w.Segments;
        var w = new Work
        {
            WorkCode = Random.Shared.Next(600000, 699999), DivisionId = divisionId, WorkDesc = $"{workClass} work", UserCode = "T",
            MajorAccCode = seg.Major, SubMajorAccCode = seg.SubMajor, MinorAccCode = seg.Minor,
            SubMinorAccCode = seg.SubMinor, DetailAccCode = seg.Detail, ObjectAccCode = seg.Object,
            WorkClass = workClass, EnteredVia = "LEGACY_OPENING",
        };
        ctx.Works.Add(w);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return w.Id;
    }

    // ------------------------------------------------------------------ DB-06, DB-27

    [Fact]
    public async Task DB06_version_3_without_version_2_is_refused()
    {
        var id = await CommittedApprovalAsync(1000);
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_REVISE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        (await RefusedAsync(() => ExecAsync(c, tx, ApprovalVersion(id, 3, att, 1200)))).MessageText.Should().Be("DB-06");
    }

    [Fact]
    public async Task DB06_header_current_version_not_equal_to_the_highest_version_is_refused_at_commit()
    {
        var id = await CommittedApprovalAsync(1000);
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_REVISE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        await ExecAsync(c, tx, ApprovalVersion(id, 2, att, 1200));
        // current_version_no left at 1 while version 2 exists.
        (await RefusedAsync(() => tx.CommitAsync())).MessageText.Should().Be("DB-06");
    }

    [Fact]
    public async Task DB06_header_without_its_version_1_is_refused_at_commit()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_CREATE");
        await using var _ = c;
        await ScalarAsync(c, tx, ApprovalHeader(_w.WorkAId, 1));
        (await RefusedAsync(() => tx.CommitAsync())).MessageText.Should().Be("DB-06");
    }

    [Fact]
    public async Task DB06_revision_with_header_moved_to_version_2_commits_and_keeps_version_1()
    {
        var id = await CommittedApprovalAsync(1000);
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_REVISE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        await ExecAsync(c, tx, ApprovalVersion(id, 2, att, 1200));
        await ExecAsync(c, tx, $"UPDATE budget.admin_approval SET current_version_no = 2 WHERE admin_approval_id = {id}");
        await tx.CommitAsync();
        var (c2, tx2) = await _w.BeginAsync(_w.ItCell);
        await using var __ = c2;
        (await ScalarAsync(c2, tx2, $"SELECT count(*) FROM budget.admin_approval_version WHERE admin_approval_id = {id}")).Should().Be(2);
    }

    [Fact]
    public async Task DB27_update_and_delete_of_versions_and_attachments_are_refused()
    {
        var id = await CommittedApprovalAsync(1000);
        foreach (var sql in new[]
                 {
                     $"UPDATE budget.admin_approval_version SET amount_sanctioned = 1 WHERE admin_approval_id = {id}",
                     $"DELETE FROM budget.admin_approval_version WHERE admin_approval_id = {id}",
                     "UPDATE budget.attachment SET original_file_name = 'x'",
                     "TRUNCATE budget.attachment CASCADE",
                     "TRUNCATE budget.technical_sanction_version CASCADE",
                     "INSERT INTO budget.delegation_limit (authority_level, max_amount, effective_from, source_ref) VALUES ('EE', 100, current_date, 'R-06'); UPDATE budget.delegation_limit SET max_amount = 5",
                     $"DELETE FROM budget.admin_approval WHERE admin_approval_id = {id}",
                 })
        {
            var (c, tx) = await _w.BeginAsync(_w.ItCell);
            await using var _ = c;
            (await RefusedAsync(() => ExecAsync(c, tx, sql))).MessageText.Should().Be("FORBIDDEN", sql);
        }
    }

    // ------------------------------------------------------------------ DB-09 (VR-02, DV-15, BUD-074)

    [Fact]
    public async Task DV15_DB09_sanctions_at_exactly_105_percent_commit()
    {
        var aa = await CommittedApprovalAsync(1000);
        var (c, tx) = await SanctionAsync(aa, 1050, 1, "TS/1");
        await using var _ = c;
        await tx.CommitAsync();
    }

    [Fact]
    public async Task DV15_DB09_one_rupee_above_105_percent_is_refused_at_commit_with_M06()
    {
        var aa = await CommittedApprovalAsync(1000);
        var (c, tx) = await SanctionAsync(aa, 1050, 1, "TS/1");
        await using (c) await tx.CommitAsync();
        var (c2, tx2) = await SanctionAsync(aa, 1, 2, "TS/2");
        await using var _ = c2;
        (await RefusedAsync(() => tx2.CommitAsync())).MessageText.Should().Be("M-06", "the check is deferred to commit");
    }

    [Fact]
    public async Task BUD074_upward_approval_revision_readmits_a_refused_sanction()
    {
        var aa = await CommittedApprovalAsync(1000);
        var (c, tx) = await SanctionAsync(aa, 1050, 1, "TS/1");
        await using (c) await tx.CommitAsync();
        var (c2, tx2) = await SanctionAsync(aa, 500, 2, "TS/2");
        await using (c2) await RefusedAsync(() => tx2.CommitAsync());

        var (c3, tx3) = await _w.BeginAsync(_w.DivisionA, "AA_REVISE");
        await using (c3)
        {
            var att = await ScalarAsync(c3, tx3, Attachment());
            await ExecAsync(c3, tx3, ApprovalVersion(aa, 2, att, 2000));
            await ExecAsync(c3, tx3, $"UPDATE budget.admin_approval SET current_version_no = 2 WHERE admin_approval_id = {aa}");
            await tx3.CommitAsync();
        }
        var (c4, tx4) = await SanctionAsync(aa, 500, 2, "TS/2");
        await using var _ = c4;
        await tx4.CommitAsync(); // 1550 <= 105% of 2000
    }

    [Fact]
    public async Task DB09_sanction_revision_above_the_ceiling_is_refused_at_commit()
    {
        var aa = await CommittedApprovalAsync(1000);
        long ts;
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        await using (c)
        {
            var att = await ScalarAsync(c, tx, Attachment());
            ts = await ScalarAsync(c, tx, SanctionHeader(_w.WorkAId, "AA", aa, 1));
            await ExecAsync(c, tx, SanctionVersion(ts, 1, att, 1000, "TS/1"));
            await tx.CommitAsync();
        }
        var (c2, tx2) = await _w.BeginAsync(_w.DivisionA, "TS_REVISE");
        await using var _ = c2;
        var att2 = await ScalarAsync(c2, tx2, Attachment());
        await ExecAsync(c2, tx2, SanctionVersion(ts, 2, att2, 1051, "TS/1"));
        await ExecAsync(c2, tx2, $"UPDATE budget.technical_sanction SET current_version_no = 2 WHERE technical_sanction_id = {ts}");
        (await RefusedAsync(() => tx2.CommitAsync())).MessageText.Should().Be("M-06");
    }

    // ------------------------------------------------------------------ DB-08 (DV-12), attachment CHECKs

    [Fact]
    public async Task DV12_DB08_approval_copy_of_supporting_class_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_CREATE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment("SUPPORTING", "docx"));
        var id = await ScalarAsync(c, tx, ApprovalHeader(_w.WorkAId, 1));
        (await RefusedAsync(() => ExecAsync(c, tx, ApprovalVersion(id, 1, att, 1000)))).MessageText.Should().Be("DB-08");
    }

    [Fact]
    public async Task DV12_evidence_attachment_of_a_word_or_excel_file_is_refused_by_check()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "UPLOAD");
        await using var _ = c;
        (await RefusedAsync(() => ScalarAsync(c, tx, Attachment("EVIDENCE", "xlsx"))))
            .ConstraintName.Should().Be("ck_attachment_evidence_format");
    }

    [Fact]
    public async Task T22_attachment_size_above_10485760_is_refused_and_exactly_the_limit_accepted()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "UPLOAD");
        await using var _ = c;
        await ScalarAsync(c, tx, Attachment().Replace("1024", "10485760"));
        (await RefusedAsync(() => ScalarAsync(c, tx, Attachment().Replace("1024", "10485761"))))
            .ConstraintName.Should().Be("ck_attachment_size");
    }

    [Fact]
    public async Task DB08_demand_line_ce_certificate_of_supporting_class_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "DEMAND_SAVE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment("SUPPORTING", "pdf"));
        (await RefusedAsync(() => ExecAsync(c, tx,
                "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, ce_certificate_attachment_id) " +
                $"VALUES ({_w.WorksProposalAId}, 'WORK', 1, {_w.WorkAId}, {_w.AccountCodeId}, 100, {att})")))
            .MessageText.Should().Be("DB-08");
    }

    // ------------------------------------------------------------------ DB-07 (DV-14)

    [Fact]
    public async Task DV14_DB07_reference_of_another_approval_of_the_Division_is_refused()
    {
        await CommittedApprovalAsync(1000, reference: "AA/77");
        var otherWork = await NewWorkAsync("AA", _w.HierarchyA.DivisionId, _w.DivisionA);
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_CREATE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        var id = await ScalarAsync(c, tx, ApprovalHeader(otherWork, 2));
        (await RefusedAsync(() => ExecAsync(c, tx, ApprovalVersion(id, 1, att, 500, "AA/77")))).MessageText.Should().Be("DB-07");
    }

    [Fact]
    public async Task DV14_DB07_revision_of_the_same_approval_may_repeat_its_reference()
    {
        var id = await CommittedApprovalAsync(1000, reference: "AA/77");
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "AA_REVISE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        await ExecAsync(c, tx, ApprovalVersion(id, 2, att, 1100, "AA/77"));
        await ExecAsync(c, tx, $"UPDATE budget.admin_approval SET current_version_no = 2 WHERE admin_approval_id = {id}");
        await tx.CommitAsync();
    }

    [Fact]
    public async Task DB07_ts_number_of_another_sanction_of_the_Division_is_refused()
    {
        var aa = await CommittedApprovalAsync(10000);
        var (c, tx) = await SanctionAsync(aa, 100, 1, "TS/9");
        await using (c) await tx.CommitAsync();
        var (c2, tx2) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        await using var _ = c2;
        var att = await ScalarAsync(c2, tx2, Attachment());
        var ts = await ScalarAsync(c2, tx2, SanctionHeader(_w.WorkAId, "AA", aa, 2));
        (await RefusedAsync(() => ExecAsync(c2, tx2, SanctionVersion(ts, 1, att, 100, "TS/9")))).MessageText.Should().Be("DB-07");
    }

    // ------------------------------------------------------------------ DB-04, AMR one sanction (DV-15)

    [Fact]
    public async Task DV15_second_sanction_on_an_AMR_work_is_refused_by_the_partial_unique_key()
    {
        var amr = await NewWorkAsync("AMR", _w.HierarchyA.DivisionId, _w.DivisionA);
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        await using var _ = c;
        var att = await ScalarAsync(c, tx, Attachment());
        var ts = await ScalarAsync(c, tx, SanctionHeader(amr, "AMR", null, 1));
        await ExecAsync(c, tx, SanctionVersion(ts, 1, att, 100, "TS/A1"));
        (await RefusedAsync(() => ScalarAsync(c, tx, SanctionHeader(amr, "AMR", null, 2))))
            .ConstraintName.Should().Be("ux_technical_sanction_amr_work");
    }

    [Fact]
    public async Task DB04_sanction_tab_other_than_the_work_class_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        await using var _ = c;
        (await RefusedAsync(() => ScalarAsync(c, tx, SanctionHeader(_w.WorkAId, "AMR", null, 1)))).MessageText.Should().Be("DB-04");
    }

    [Fact]
    public async Task DB04_approval_held_by_another_Division_office_is_refused()
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionB, "AA_CREATE");
        await using var _ = c;
        (await RefusedAsync(() => ScalarAsync(c, tx,
                "INSERT INTO budget.admin_approval (work_id, division_office_id, serial_no, current_version_no) " +
                $"VALUES ({_w.WorkAId}, {_w.DivisionBOfficeId}, 1, 1) RETURNING admin_approval_id")))
            .MessageText.Should().Be("DB-04");
    }

    // ------------------------------------------------------------------ DB-05 (DV-13 estimate code part)

    [Fact]
    public async Task DV13_DB05_estimate_code_written_through_the_function_once_and_never_directly()
    {
        var aa = await CommittedApprovalAsync(1000);
        long ts;
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "TS_CREATE");
        await using (c)
        {
            var att = await ScalarAsync(c, tx, Attachment());
            ts = await ScalarAsync(c, tx, SanctionHeader(_w.WorkAId, "AA", aa, 1));
            await ExecAsync(c, tx, SanctionVersion(ts, 1, att, 1000, "TS/1"));
            await tx.CommitAsync();
        }
        // Directly: refused even with no code yet recorded.
        var (c2, tx2) = await _w.BeginAsync(_w.ItCell, "FA_ESTIMATE_CODE");
        await using (c2)
            (await RefusedAsync(() => ExecAsync(c2, tx2,
                    $"UPDATE budget.technical_sanction SET estimate_code = 'E-1', estimate_code_at = now() WHERE technical_sanction_id = {ts}")))
                .MessageText.Should().Be("DB-05");
        // Through the function: accepted once.
        var (c3, tx3) = await _w.BeginAsync(_w.ItCell, "FA_ESTIMATE_CODE");
        await using (c3)
        {
            await ExecAsync(c3, tx3, $"SELECT budget.fn_record_estimate_code({ts}, 'E-1')");
            await tx3.CommitAsync();
        }
        // A second code: refused.
        var (c4, tx4) = await _w.BeginAsync(_w.ItCell, "FA_ESTIMATE_CODE");
        await using var _ = c4;
        (await RefusedAsync(() => ExecAsync(c4, tx4, $"SELECT budget.fn_record_estimate_code({ts}, 'E-2')")))
            .MessageText.Should().Be("DB-05");
    }

    [Fact]
    public async Task DB05_correction_flag_clearance_is_set_once()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "AA_REVISE");
        await using var _ = c;
        var flag = await ScalarAsync(c, tx,
            $"INSERT INTO budget.correction_flag (work_id, reason, is_critical) VALUES ({_w.WorkAId}, 'AA_REDUCED', false) RETURNING correction_flag_id");
        await ExecAsync(c, tx,
            $"UPDATE budget.correction_flag SET cleared_at = now(), cleared_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE correction_flag_id = {flag}");
        (await RefusedAsync(() => ExecAsync(c, tx,
                $"UPDATE budget.correction_flag SET cleared_at = now() + interval '1 day' WHERE correction_flag_id = {flag}")))
            .MessageText.Should().Be("DB-05");
    }

    // ------------------------------------------------------------------ DB-28 and V-04 v2

    [Fact]
    public async Task DB28_attachment_without_actor_context_is_refused()
    {
        await using var c = new NpgsqlConnection(_w.ConnectionString);
        await c.OpenAsync();
        await using var tx = await c.BeginTransactionAsync();
        (await RefusedAsync(() => ScalarAsync(c, tx, Attachment()))).MessageText.Should().Be("ACTOR");
    }

    [Fact]
    public async Task V04_v2_shows_the_current_approval_amount_and_sanctions_to_date_on_the_work_line()
    {
        var aa = await CommittedApprovalAsync(1000, reference: "AA/5");
        var (c, tx) = await SanctionAsync(aa, 400, 1, "TS/1");
        await using (c) await tx.CommitAsync();
        var (c2, tx2) = await _w.BeginAsync(_w.DivisionA, "DEMAND_SAVE");
        await using var _ = c2;
        var line = await ScalarAsync(c2, tx2,
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id) " +
            $"VALUES ({_w.WorksProposalAId}, 'WORK', 1, {_w.WorkAId}, {_w.AccountCodeId}) RETURNING demand_line_id");
        (await ScalarAsync(c2, tx2,
                $"SELECT count(*) FROM budget.v_demand_line_display WHERE demand_line_id = {line} AND admin_approval_id = {aa} " +
                "AND aa_amount_sanctioned = 1000 AND aa_reference_no = 'AA/5' AND ts_count = 1 AND ts_amount_to_date = 400"))
            .Should().Be(1);
    }

    // ------------------------------------------------------------------ DB-09 serialisation (BudgetP2TsCeilingLock)

    [Fact]
    public async Task DB09_two_concurrent_sanctions_that_fit_alone_but_not_together_second_commit_refused_M06()
    {
        var aa = await CommittedApprovalAsync(1000);
        var (c1, tx1) = await SanctionAsync(aa, 600, 1, "TS/1");
        var second = Task.Run(async () =>
        {
            var (c2, tx2) = await SanctionAsync(aa, 600, 2, "TS/2");
            await using (c2)
            {
                try { await tx2.CommitAsync(); return (PostgresException?)null; }
                catch (PostgresException e) { return e; }
            }
        });
        await Task.Delay(750);
        second.IsCompleted.Should().BeFalse("the second writer waits for the first transaction's lock");
        await using (c1) await tx1.CommitAsync();

        var refused = await second;
        refused.Should().NotBeNull();
        refused!.MessageText.Should().Be("M-06");

        var (c3, tx3) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c3;
        (await ScalarAsync(c3, tx3,
                "SELECT coalesce(sum(v.ts_amount), 0) FROM budget.technical_sanction t JOIN budget.technical_sanction_version v " +
                $"ON v.technical_sanction_id = t.technical_sanction_id WHERE t.admin_approval_id = {aa}"))
            .Should().Be(600, "the first sanction stays committed");
    }

    [Fact]
    public async Task DB09_ceiling_check_takes_the_per_approval_advisory_lock_before_summing()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        await using var cmd = new NpgsqlCommand("SELECT pg_get_functiondef('budget.fn_ts_ceiling_check()'::regprocedure)", c, tx);
        var def = (string)(await cmd.ExecuteScalarAsync())!;
        var lockAt = def.IndexOf("pg_advisory_xact_lock(hashtext('budget.ts_ceiling')", StringComparison.Ordinal);
        lockAt.Should().BePositive();
        lockAt.Should().BeLessThan(def.IndexOf("sum(v.ts_amount)", StringComparison.Ordinal));
    }
}
