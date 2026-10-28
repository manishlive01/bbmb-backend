using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Shared;

/// <summary>
/// A fully migrated database of its own with a small budget world: Finance, Secretariat, one CE, one Circle,
/// two Divisions (A and B) in T-13; years 2026 and 2027; an OPEN cycle 2026/2027; one account code; one work
/// in each Division; a WORKS proposal of Division A at S-01. Actors for the IT Cell and each office.
/// </summary>
public sealed class SpineWorld
{
    public required string ConnectionString { get; init; }
    public required long FinanceOfficeId { get; init; }
    public required long SecretariatOfficeId { get; init; }
    public required long CeOfficeId { get; init; }
    public required long CircleOfficeId { get; init; }
    public required long DivisionAOfficeId { get; init; }
    public required long DivisionBOfficeId { get; init; }
    public required TestMasters.Hierarchy HierarchyA { get; init; }
    public required TestMasters.Hierarchy HierarchyB { get; init; }
    public required long CycleId { get; init; }
    public required long AccountCodeId { get; init; }
    public required TestMasters.Segments Segments { get; init; }
    public required int WorkAId { get; init; }
    public required int WorkBId { get; init; }
    public required long WorksProposalAId { get; init; }
    public required FakeBudgetActorSource ItCell { get; init; }
    public required FakeBudgetActorSource DivisionA { get; init; }
    public required FakeBudgetActorSource DivisionB { get; init; }
    public required FakeBudgetActorSource Circle { get; init; }

    public BudgetContext NewContext() => BudgetDatabaseFixture.NewBudgetContext(ConnectionString);

    private static FakeBudgetActorSource As(FakeBudgetActorSource source, long officeId, string level, string role) =>
        new(source.Actor! with { OfficeId = officeId, OfficeLevel = level, RoleCode = role });

    /// <param name="withCycle">False leaves T-14 and T-29 empty (CycleId and WorksProposalAId are 0).</param>
    /// <param name="migrateTo">Target migration; null migrates fully. Must be at or after BudgetP1ProposalSpine.</param>
    public static async Task<SpineWorld> CreateAsync(BudgetDatabaseFixture fixture, string name, bool withCycle = true, string? migrateTo = null)
    {
        var cs = await fixture.CreateFreshDatabaseAsync(name);
        await fixture.MigrateToAsync(cs, migrateTo);
        var a = await TestMasters.InsertDivisionAsync(cs);
        var b = await TestMasters.InsertDivisionAsync(cs);
        var seg = await TestMasters.InsertSegmentsAsync(cs);
        var it = await TestActor.CreateAsync(fixture, connectionString: cs, name: "IT Officer");

        await using var ctx = BudgetDatabaseFixture.NewBudgetContext(cs);
        await ctx.Database.ExecuteSqlRawAsync(
            "INSERT INTO budget.irr_pwr (irr_pwr_code, irr_pwr_desc, modification_date, user_code) VALUES ('I', 'Irrigation', now(), 'T') ON CONFLICT DO NOTHING; " +
            "INSERT INTO budget.fa_financial_years (fin_year_closing, start_date, end_date) VALUES " +
            "(2026, '2025-04-01', '2026-03-31'), (2027, '2026-04-01', '2027-03-31'), (2028, '2027-04-01', '2028-03-31') ON CONFLICT DO NOTHING");

        long finance, secretariat, ce, circle, divA, divB, cycle, accountCodeId;
        await using (var tx = await new BudgetActionScope(ctx, it).BeginAsync("TEST_SETUP"))
        {
            async Task<long> Office(BudgetOffice o) { ctx.BudgetOffices.Add(o); await ctx.SaveChangesAsync(); return o.OfficeId; }
            finance = await Office(new BudgetOffice { OfficeLevel = "FINANCE", OfficeName = "Finance" });
            secretariat = await Office(new BudgetOffice { OfficeLevel = "SECRETARIAT", OfficeName = "Secretariat", ParentOfficeId = finance });
            ce = await Office(new BudgetOffice { OfficeLevel = "CE", CeId = a.CeId, WingCode = "I", ParentOfficeId = secretariat });
            circle = await Office(new BudgetOffice { OfficeLevel = "CIRCLE", CircleId = a.CrId, ParentOfficeId = ce });
            divA = await Office(new BudgetOffice { OfficeLevel = "DIVISION", DivisionId = a.DivisionId, ParentOfficeId = circle });
            divB = await Office(new BudgetOffice { OfficeLevel = "DIVISION", DivisionId = b.DivisionId, ParentOfficeId = circle });

            cycle = 0;
            if (withCycle)
            {
                var c = new BudgetCycle { CurrentFinYear = 2026, NextFinYear = 2027, IsFirstCycle = true, Status = "OPEN" };
                ctx.BudgetCycles.Add(c);
                await ctx.SaveChangesAsync();
                cycle = c.CycleId;
            }

            await ctx.Database.ExecuteSqlRawAsync(
                "INSERT INTO budget.budget_head_master (major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, " +
                "detail_acc_code, object_acc_code, head_desc, party_emp_required, origin, modification_date, user_code) " +
                "VALUES ({0}, {1}, {2}, {3}, {4}, {5}, 'Test head', 'N', 'LEGACY', now(), 'T')",
                seg.Major, seg.SubMajor, seg.Minor, seg.SubMinor, seg.Detail, seg.Object);
            accountCodeId = await ctx.HeadOfAccounts.Where(h => h.MajorAccCode == seg.Major && h.MinorAccCode == seg.Minor)
                .Select(h => h.AccountCodeId).SingleAsync();
            // From BudgetP2AccountCodesAndAssignment a head-wise line needs a live T-21 assignment (DB-02). The world's
            // code is assigned to every Division, Circle and CE office, as the L-05 load would (source R04_LOAD).
            // Skipped when the world is migrated to an earlier target.
#pragma warning disable EF1002 // the interpolated values are ids read from the database above
            await ctx.Database.ExecuteSqlRawAsync(
                "DO $$ BEGIN IF to_regclass('budget.account_head_assignment') IS NOT NULL THEN " +
                "EXECUTE 'INSERT INTO budget.account_head_assignment (office_id, account_code_id, source) " +
                $"SELECT o, {accountCodeId}, ''R04_LOAD'' FROM unnest(ARRAY[{ce}, {circle}, {divA}, {divB}]::bigint[]) AS o'; " +
                "END IF; END $$");
#pragma warning restore EF1002
            await tx.CommitAsync();
        }

        var divisionA = As(it, divA, "DIVISION", "DDO");
        var divisionB = As(it, divB, "DIVISION", "DDO");

        async Task<int> NewWork(FakeBudgetActorSource actor, int divisionId, int code)
        {
            // Insert via raw SQL (not EF) so only the columns that exist at the migrateTo target are written:
            // the Work entity carries the P4 work-code columns, which are absent at an early target such as
            // BudgetP1FiguresAndRecommendations (end-of-run fix for S114). work_code is null at entry (P4 §4.6);
            // the [A] actor columns are trigger-filled.
            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            await DbActor.SetAsync(conn, tx, actor.Actor!.EmployeeId, actor.Actor.ChargeId, actor.Actor.OfficeId,
                role: actor.Actor.RoleCode, action: "WORKS_ENTER", name: actor.Actor.ActorName);
            // The work_code column differs by migrate-to target: before P4 it is int NOT NULL (so an integer
            // code is given); from P4 (BudgetP4WorkCodeConformance) it is a nullable varchar with pairing CHECKs
            // (so it must be NULL at entry, P4 §4.6). Detect the P4 shape by the presence of work_code_origin.
            bool postP4;
            await using (var probe = new NpgsqlCommand(
                "SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema='budget' " +
                "AND table_name='work' AND column_name='work_code_origin')", conn, tx))
                postP4 = (bool)(await probe.ExecuteScalarAsync())!;
            var workCodeValue = postP4 ? "NULL" : code.ToString(System.Globalization.CultureInfo.InvariantCulture);
#pragma warning disable EF1002
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO budget.work (work_code, division_id, work_desc, user_code, work_class, entered_via, pre_ddd, " +
                "major_acc_code, sub_major_acc_code, minor_acc_code, sub_minor_acc_code, detail_acc_code, object_acc_code, " +
                "original_budget, revised_budget, proposed_budget, remarks) " +
                $"VALUES ({workCodeValue}, {divisionId}, 'Work {code}', 'T', 'AA', 'LEGACY_OPENING', false, " +
                $"'{seg.Major}', '{seg.SubMajor}', '{seg.Minor}', '{seg.SubMinor}', '{seg.Detail}', '{seg.Object}', " +
                "'', '', '', '') RETURNING id", conn, tx);
#pragma warning restore EF1002
            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await tx.CommitAsync();
            return id;
        }
        var workA = await NewWork(divisionA, a.DivisionId, 500001);
        var workB = await NewWork(divisionB, b.DivisionId, 500002);

        long proposal = 0;
        if (withCycle)
        await using (var pctx = BudgetDatabaseFixture.NewBudgetContext(cs))
        await using (var ptx = await new BudgetActionScope(pctx, it).BeginAsync("OPEN_PROPOSALS"))
        {
            var p = new BudgetProposal { CycleId = cycle, Category = "WORKS", OriginOfficeId = divA, HolderOfficeId = divA, StateCode = "S-01" };
            pctx.BudgetProposals.Add(p);
            await pctx.SaveChangesAsync();
            await ptx.CommitAsync();
            proposal = p.ProposalId;
        }

        return new SpineWorld
        {
            ConnectionString = cs, FinanceOfficeId = finance, SecretariatOfficeId = secretariat, CeOfficeId = ce,
            CircleOfficeId = circle, DivisionAOfficeId = divA, DivisionBOfficeId = divB, HierarchyA = a, HierarchyB = b,
            CycleId = cycle, AccountCodeId = accountCodeId, Segments = seg, WorkAId = workA, WorkBId = workB,
            WorksProposalAId = proposal, ItCell = it, DivisionA = divisionA, DivisionB = divisionB,
            Circle = As(it, circle, "CIRCLE", "SE"),
        };
    }

    /// <summary>Opens a raw connection and transaction under <paramref name="actor"/> (DDD §7.1 keys).</summary>
    public async Task<(NpgsqlConnection Conn, NpgsqlTransaction Tx)> BeginAsync(FakeBudgetActorSource actor, string action = "TEST")
    {
        var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        var tx = await conn.BeginTransactionAsync();
        await DbActor.SetAsync(conn, tx, actor.Actor!.EmployeeId, actor.Actor.ChargeId, actor.Actor.OfficeId,
            role: actor.Actor.RoleCode, action: action, name: actor.Actor.ActorName);
        return (conn, tx);
    }
}
