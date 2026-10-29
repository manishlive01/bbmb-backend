using System.Security.Claims;
using System.Text.Json;
using Auth.Authorization.Handlers;
using BBMB.BudgetService.Features.BudgetProcess.AuthzConfig;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Rules;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Rules;
using BBMB.BudgetService.Features.BulkUploads;
using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.ServiceDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using OfficeOpenXml;
namespace BBMB.BudgetService.Tests.Exit;
/// <summary>
/// P1 exit sweep (design §12 P1 row, §14): the exit-evidence bullets and §14 P1 tests that no earlier item's
/// tests demonstrated. Each test name carries the identifier it demonstrates (BUD-264).
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed class P1ExitTests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;
    private static readonly IConfiguration Config = new ConfigurationBuilder().Build();
    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"p1x_{Guid.NewGuid():N}"[..20]);
    public Task DisposeAsync() => Task.CompletedTask;
    private FakeBudgetActorSource At(long officeId, string level, string role) =>
        new(_w.ItCell.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });

    private async Task<Response> AddAsync(IBudgetActorSource actor, AddDemandLineDto dto, long? proposalId = null, IHttpContextAccessor? http = null)
    {
        await using var ctx = _w.NewContext();
        return await new AddDemandLineHandler(ctx, actor, new BudgetActionScope(ctx, actor), TestServices.OfficeScope(ctx),
                TestServices.AllowAll(), http ?? new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, Config,
                new DemandLineRules(new DemandLineRuleInputsLoader(ctx)), new WorkRules(ctx))
            .Handle(new AddDemandLineCommand(proposalId ?? _w.WorksProposalAId, dto), default);
    }

    private async Task<string> MoveAsync(FakeBudgetActorSource actor, long id, string action, string? remark = null)
    {
        await using var ctx = _w.NewContext();
        var r = await TestServices.Transitions(ctx, actor).MoveAsync(id, action, remark);
        r.IsSucced.Should().BeTrue($"{action}: {r.Message}");
        return ((Response<ProposalStateResponseDto>)r).Data!.StateCode;
    }

    private async Task<long> NewProposalAsync(string category, long officeId)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("OPEN_PROPOSALS");
        var p = new BudgetProposal { CycleId = _w.CycleId, Category = category, OriginOfficeId = officeId, HolderOfficeId = officeId, StateCode = "S-01" };
        ctx.BudgetProposals.Add(p);
        await ctx.SaveChangesAsync();
        await tx.CommitAsync();
        return p.ProposalId;
    }

    // ---------------------------------------------------------------------------------------------
    // §12 "transition table round-trip incl. every recall row": T04 and T04b/T09c/T13c are in
    // SubmitRecallTests; here T09, T09b, T13, T13b and T17 against the seeded T-16.
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task BUD214_every_recall_row_round_trips_T09_T09b_T13_T13b_T17()
    {
        var ce = At(_w.CeOfficeId, "CE", "CE");
        var sec = At(_w.SecretariatOfficeId, "SECRETARIAT", "SECRETARY");
        var id = _w.WorksProposalAId;
        (await MoveAsync(_w.DivisionA, id, "SUBMIT")).Should().Be("S-02");
        (await MoveAsync(_w.Circle, id, "OPEN")).Should().Be("S-03");
        (await MoveAsync(_w.Circle, id, "FORWARD")).Should().Be("S-05");
        (await MoveAsync(_w.Circle, id, "RECALL")).Should().Be("S-03", "T09");
        (await MoveAsync(_w.Circle, id, "FORWARD")).Should().Be("S-05");
        (await MoveAsync(ce, id, "OPEN")).Should().Be("S-06");
        (await MoveAsync(ce, id, "FORWARD")).Should().Be("S-07");
        (await MoveAsync(ce, id, "RECALL")).Should().Be("S-06", "T13");
        (await MoveAsync(ce, id, "FORWARD")).Should().Be("S-07");
        (await MoveAsync(sec, id, "OPEN")).Should().Be("S-08");
        (await MoveAsync(sec, id, "FORWARD_FINANCE")).Should().Be("S-09");
        (await MoveAsync(sec, id, "RECALL")).Should().Be("S-08", "T17");

        var circleDna = await NewProposalAsync("DNA", _w.CircleOfficeId);
        (await MoveAsync(_w.Circle, circleDna, "SUBMIT")).Should().Be("S-05");
        (await MoveAsync(_w.Circle, circleDna, "RECALL")).Should().Be("S-01", "T09b");
        var ceDna = await NewProposalAsync("DNA", _w.CeOfficeId);
        (await MoveAsync(ce, ceDna, "SUBMIT")).Should().Be("S-07");
        (await MoveAsync(ce, ceDna, "RECALL")).Should().Be("S-01", "T13b");
    }

    // ---------------------------------------------------------------------------------------------
    // §12 "officer with two active Budget charges and no X-Budget-Charge → M-20; with the header naming
    // one → that charge in T-51", end to end through HttpBudgetActorSource and ChargeResolver.
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task D12_two_charges_without_header_is_M20_and_the_named_charge_is_recorded_in_T51()
    {
        var (employeeId, firstCharge, userId) = await DbActor.NewEmployeeWithChargeAsync(fixture,
            divisionId: _w.HierarchyA.DivisionId, name: "Two Charge DDO", connectionString: _w.ConnectionString);
        var secondCharge = Guid.NewGuid();
        await using (var emp = BudgetDatabaseFixture.NewEmployeeContext(_w.ConnectionString))
        {
            emp.EmployeeCharges.Add(new EmployeeCharge
            {
                Id = secondCharge, EmployeeId = employeeId, ProjectCode = "BU", CEId = 0, DivisionId = _w.HierarchyA.DivisionId,
                ChargeTime = DateTime.UtcNow.AddDays(-1), IsActive = true, IsChargeExpire = false,
            });
            await emp.SaveChangesAsync();
        }
        HttpContextAccessor Http(string? chargeHeader)
        {
            var http = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "Bearer")),
            };
            http.Items[BudgetActingRole.ItemKey] = "DDO";
            if (chargeHeader is not null) http.Request.Headers[BudgetChargeResult.HeaderName] = chargeHeader;
            return new HttpContextAccessor { HttpContext = http };
        }
        var config = new ExitConfigProvider("BU");
        async Task<Response> AddAs(string? header)
        {
            var http = Http(header);
            await using var ctx = _w.NewContext();
            var empCtx = BudgetDatabaseFixture.NewEmployeeContext(_w.ConnectionString);
            var officeScope = TestServices.OfficeScope(ctx);
            var source = new HttpBudgetActorSource(http, new ChargeResolver(http, empCtx, config, officeScope), officeScope, config, empCtx,
                NullLogger<HttpBudgetActorSource>.Instance);
            return await AddAsync(source, new AddDemandLineDto { LineType = "WORK", WorkId = _w.WorkAId, RbeAmount = 1 }, http: http);
        }
        (await AddAs(null)).MessageId.Should().Be("M-20", "two current Budget charges and none named (D-12)");
        var saved = await AddAs(secondCharge.ToString());
        saved.IsSucced.Should().BeTrue(saved.Message);
        var lineId = ((Response<SaveDemandLineResponseDto>)saved).Data!.DemandLineId;
        await using var c = _w.NewContext();
        var audit = await c.AuditLog.AsNoTracking().Where(a => a.TableName == "demand_line" && a.RecordKey.Contains(lineId.ToString()) && a.Operation == "I").ToListAsync();
        audit.Should().NotBeEmpty().And.OnlyContain(a => a.ActorChargeId == secondCharge && a.ActorEmployeeId == employeeId);
        firstCharge.Should().NotBe(secondCharge);
    }

    // ---------------------------------------------------------------------------------------------
    // §14 bulk upload: a headofaccountmaster row 7 refused by a T-07 database rule → "Row 7: M-…", no table
    // or constraint name, nothing saved. Every real T-07 constraint is pre-checked by the arm (segment sets,
    // duplicates in the sheet and in the Manual), so a test-only trigger on a fresh database stands in for a
    // T-07 rule raising M-02; the PK itself is shown to map to M-02 in the next test.
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task BUD012_headofaccountmaster_row_7_refusal_is_Row_7_M_only_and_nothing_is_saved()
    {
        var cs = await fixture.CreateFreshDatabaseAsync($"bulk7_{Guid.NewGuid():N}"[..20]);
        await fixture.MigrateToAsync(cs, null);
        var seg = await TestMasters.InsertSegmentsAsync(cs);
        var objects = Enumerable.Range(0, 6).Select(i => (701 + i).ToString()).ToList();
        await using (var conn = new NpgsqlConnection(cs))
        {
            await conn.OpenAsync();
            var sql = string.Join(" ", objects.Select(o => $"INSERT INTO budget.fa_object_head VALUES ('{o}', 'OB', now(), 'TEST') ON CONFLICT DO NOTHING;")) +
                      $" CREATE FUNCTION budget.test_refuse_row7() RETURNS trigger LANGUAGE plpgsql AS $f$ BEGIN " +
                      $"IF NEW.object_acc_code = '{objects[5]}' THEN RAISE EXCEPTION 'M-02' USING ERRCODE = 'P0001'; END IF; RETURN NEW; END $f$;" +
                      " CREATE TRIGGER test_refuse_row7 BEFORE INSERT ON budget.budget_head_master FOR EACH ROW EXECUTE FUNCTION budget.test_refuse_row7();";
            await using var cmd = new NpgsqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }
        var actor = await TestActor.CreateAsync(fixture, connectionString: cs, name: "Bulk Officer");
        ExcelPackage.License.SetNonCommercialPersonal("BBMB");
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Sheet1");
        ws.Cells[1, 1].Value = "Code"; ws.Cells[1, 2].Value = "Description";
        for (var i = 0; i < 6; i++)
        {
            ws.Cells[i + 2, 1].Value = $"{seg.Major}{seg.SubMajor}{seg.Minor}{seg.SubMinor}{seg.Detail}{objects[i]}";
            ws.Cells[i + 2, 2].Value = $"Head {i + 2}";
        }
        var bytes = package.GetAsByteArray();
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "upload.xlsx");
        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        var r = await new BulkUploadHandler(ctx, TestServices.Allow("F01.Create"), new HttpContextAccessor { HttpContext = new DefaultHttpContext() },
                NullLogger<BulkUploadHandler>.Instance, actor, new BudgetActionScope(ctx, actor))
            .Handle(new BulkUploadCommand(file, "headofaccountmaster"), default);
        r.IsSucced.Should().BeFalse();
        r.MessageId.Should().Be("M-02");
        r.Message.Should().StartWith("Row 7: M-02: ").And.NotContain("budget_head_master").And.NotContain("test_refuse_row7");
        await using var check = BudgetDatabaseFixture.NewBudgetContext(cs);
        (await check.HeadOfAccounts.AsNoTracking().CountAsync(h => h.MajorAccCode == seg.Major && h.MinorAccCode == seg.Minor)).Should().Be(0);
    }

    [Fact]
    public async Task S49_T07_primary_key_violation_maps_to_M02()
    {
        var (c, tx) = await _w.BeginAsync(_w.ItCell);
        await using var _ = c;
        var s = _w.Segments;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
            "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code) " +
            $"VALUES ('{s.Major}', '{s.SubMajor}', '{s.Minor}', '{s.SubMinor}', '{s.Detail}', '{s.Object}', 'Again', 'N', 'LEGACY', now(), 'T')", c, tx);
        var ex = (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
        BudgetMessages.FromPostgres(ex)?.MessageId.Should().Be("M-02");
    }

    // ---------------------------------------------------------------------------------------------
    // DV-11: a figure change writes its version; a direct insert into T-31 as bud_app is refused. The
    // privilege layer is conditional on the roles existing (OI-03), so the role is created before a fresh
    // database is migrated, and the OI-03 deployment grants on the employee schema are applied.
    // ---------------------------------------------------------------------------------------------
    [Fact]
    public async Task DV11_a_change_writes_its_version_and_a_direct_insert_into_T31_as_bud_app_is_refused()
    {
        await using (var admin = new NpgsqlConnection(_w.ConnectionString))
        {
            await admin.OpenAsync();
            await using var role = new NpgsqlCommand(
                "DO $$ BEGIN IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN CREATE ROLE bud_app NOLOGIN; END IF; END $$;", admin);
            await role.ExecuteNonQueryAsync();
        }
        var w = await SpineWorld.CreateAsync(fixture, $"dv11_{Guid.NewGuid():N}"[..20]);
        await using (var admin = new NpgsqlConnection(w.ConnectionString))
        {
            await admin.OpenAsync();
            await using var grants = new NpgsqlCommand(
                "GRANT USAGE ON SCHEMA budget, employee TO bud_app; " +
                "GRANT SELECT ON employee.employee, employee.employee_charge TO bud_app;", admin);
            await grants.ExecuteNonQueryAsync();
        }
        // A HEAD line: fn_demand_line_rules (not SECURITY DEFINER) reads budget.work for DB-04 on a WORK line,
        // and no migration grants bud_app SELECT on work - raised under OI-03 (P1-36 evidence).
        long dnaId;
        await using (var ctx = w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, w.ItCell).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal { CycleId = w.CycleId, Category = "DNA", OriginOfficeId = w.DivisionAOfficeId, HolderOfficeId = w.DivisionAOfficeId, StateCode = "S-01" };
            ctx.BudgetProposals.Add(p);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            dnaId = p.ProposalId;
        }
        long lineId;
        await using (var ctx = w.NewContext())
        await using (var tx = await new BudgetActionScope(ctx, w.DivisionA).BeginAsync("DEMAND_LINE_ADD"))
        {
            var l = new DemandLine { ProposalId = dnaId, LineType = "HEAD", AccountCodeId = w.AccountCodeId, SerialNo = 1, RbeAmount = 10 };
            ctx.DemandLines.Add(l);
            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
            lineId = l.DemandLineId;
        }
        var (c, t) = await w.BeginAsync(w.DivisionA, "DEMAND_LINE_UPDATE");
        await using (c)
        {
            await using (var setRole = new NpgsqlCommand("SET LOCAL ROLE bud_app", c, t)) await setRole.ExecuteNonQueryAsync();
            await using (var change = new NpgsqlCommand($"UPDATE budget.demand_line SET rbe_amount = 20 WHERE demand_line_id = {lineId}", c, t))
                (await change.ExecuteNonQueryAsync()).Should().Be(1);
            await using (var count = new NpgsqlCommand($"SELECT count(*) FROM budget.figure_version WHERE demand_line_id = {lineId} AND figure = 'RBE'", c, t))
                ((long)(await count.ExecuteScalarAsync())!).Should().Be(2, "the change wrote its version through the SECURITY DEFINER trigger");
            await using var direct = new NpgsqlCommand(
                "INSERT INTO budget.figure_version (demand_line_id, figure, value, level_office_id, state_code, change_source, role_code) " +
                $"VALUES ({lineId}, 'RBE', 99, {w.DivisionAOfficeId}, 'S-01', 'ENTRY', 'DDO')", c, t);
            var ex = (await FluentActions.Awaiting(() => direct.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
            ex.SqlState.Should().Be(PostgresErrorCodes.InsufficientPrivilege);
        }
    }

    // DV-18: a Division, a financial year or an employee referred to by the budget cannot be deleted.
    [Fact]
    public async Task DV18_deleting_a_referenced_division_financial_year_or_employee_is_refused()
    {
        foreach (var sql in new[]
                 {
                     $"DELETE FROM budget.division WHERE id = {_w.HierarchyA.DivisionId}",
                     "DELETE FROM budget.fa_financial_years WHERE fin_year_closing = 2026",
                     $"DELETE FROM employee.employee WHERE id = '{_w.ItCell.Actor!.EmployeeId}'",
                 })
        {
            await using var conn = new NpgsqlConnection(_w.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(sql, conn);
            var ex = (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
            ex.SqlState.Should().Be(PostgresErrorCodes.ForeignKeyViolation, sql);
        }
    }

    // §14 "Account-code tests: selecting a not-for-use code at F-07 Add returns M-05" (F-03 and F-17 are P2).
    [Fact]
    public async Task S52_F07_Add_with_a_code_marked_not_for_use_is_M05()
    {
        var dna = await NewProposalAsync("DNA", _w.DivisionAOfficeId);
        var (c, tx) = await _w.BeginAsync(_w.ItCell, "TEST_SETUP");
        await using (c)
        {
            await using var cmd = new NpgsqlCommand(
                "UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'EO/1', not_for_use_at = now(), " +
                $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {_w.AccountCodeId}", c, tx);
            await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }
        var r = await AddAsync(_w.DivisionA, new AddDemandLineDto { LineType = "HEAD", AccountCodeId = _w.AccountCodeId, RbeAmount = 1 }, dna);
        r.MessageId.Should().Be("M-05");
        ((Response<RuleFailuresDto>)r).Data!.Failures.Should().ContainSingle(f => f.Field == "accountCodeId");
    }

    // §14 "Lease authorisation by state (§4.10): each state × each role", against the default rights matrix.
    [Fact]
    public void BUD015_lease_write_right_by_state_and_role()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path),
            new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
        var rights = doc.RootElement.GetProperty("BudgetRights").GetProperty("Rights");
        bool Holds(string role, string key) => rights.TryGetProperty(key, out var roles) && roles.EnumerateArray().Any(r => r.GetString() == role);
        string[] roles = ["DDO", "SE", "CE", "SECRETARY", "AFA_BUDGET", "FIN_VIEW", "IT_CELL"];
        var expected = new Dictionary<(string State, string Category), string[]>
        {
            [("S-01", "WORKS")] = ["DDO"], [("S-04", "WORKS")] = ["DDO"],
            [("S-01", "DNA")] = ["DDO", "SE", "CE"], [("S-04", "DNA")] = ["DDO", "SE", "CE"],
            [("S-03", "WORKS")] = ["SE", "CE", "SECRETARY"], [("S-06", "WORKS")] = ["SE", "CE", "SECRETARY"], [("S-08", "WORKS")] = ["SE", "CE", "SECRETARY"],
            [("S-03", "DNA")] = ["SE", "CE", "SECRETARY"], [("S-06", "DNA")] = ["SE", "CE", "SECRETARY"], [("S-08", "DNA")] = ["SE", "CE", "SECRETARY"],
            [("S-10", "WORKS")] = ["AFA_BUDGET"], [("S-10", "DNA")] = ["AFA_BUDGET"],
        };
        foreach (var state in Enumerable.Range(1, 16).Select(i => $"S-{i:D2}"))
        foreach (var category in new[] { "WORKS", "DNA" })
        {
            var right = ProposalLease.WriteRightFor(state, category);
            var granted = roles.Where(role => right is not null && Holds(role, right)).ToArray();
            granted.Should().BeEquivalentTo(expected.GetValueOrDefault((state, category)) ?? [], $"{state} {category}");
        }
    }

    /// <summary>Minimal config provider supplying the Budget project code to the new ChargeResolver (requirement 5).</summary>
    private sealed class ExitConfigProvider(string projectCode) : IBudgetConfigProvider
    {
        public Task<BudgetConfigSnapshot> GetAsync(CancellationToken ct = default) =>
            Task.FromResult(new BudgetConfigSnapshot { ProjectCode = projectCode });
        public void Invalidate() { }
    }
}
