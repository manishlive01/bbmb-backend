using System.Text.Json;
using BBMB.BudgetService.Features.Works.Create;
using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Features.Works.Rules;
using BBMB.BudgetService.Features.Works.Update;
using BBMB.BudgetService.Features.Works.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Features.Works;

/// <summary>
/// Work master P1 rules (design §5.2 accountCodeId, §5.3 first-cycle guard, §9.3 and §11.3 rows 1-2, §14
/// revision 4 tests). Handlers are instantiated directly with the Division A DDO of a <see cref="SpineWorld"/>.
/// </summary>
[Collection(BudgetDatabaseCollection.Name)]
public sealed partial class WorkMasterP1Tests(BudgetDatabaseFixture fixture) : IAsyncLifetime
{
    private SpineWorld _w = default!;

    public async Task InitializeAsync() => _w = await SpineWorld.CreateAsync(fixture, $"wm_{Guid.NewGuid():N}"[..20]);

    public Task DisposeAsync() => Task.CompletedTask;

    private CreateWorkDto NewWorkDto(int? divisionId = null, long? accountCodeId = null) => new()
    {
        WorkDesc = "New continuing work",
        WorkClass = "AA",
        DivisionId = divisionId ?? _w.HierarchyA.DivisionId,
        AccountCodeId = accountCodeId ?? _w.AccountCodeId,
        UserCode = "T",
        CECode = "1",
        CRCode = "1",
    };

    private static async Task<Response> CreateAsync(SpineWorld w, FakeBudgetActorSource actor, CreateWorkDto dto)
    {
        await using var ctx = w.NewContext();
        return await new CreateWorkHandler(ctx, actor, new BudgetActionScope(ctx, actor), new WorkRules(ctx))
            .Handle(new CreateWorkCommand(dto), default);
    }

    private async Task<Response> UpdateAsync(int workCode, UpdateWorkDto dto, FakeBudgetActorSource? actor = null)
    {
        actor ??= _w.DivisionA;
        await using var ctx = _w.NewContext();
        return await new UpdateWorkHandler(ctx, actor, new BudgetActionScope(ctx, actor), new WorkRules(ctx))
            .Handle(new UpdateWorkCommand(workCode, dto), default);
    }

    /// <summary>An update body that keeps every stored particular of a SpineWorld work except those set.</summary>
    private static UpdateWorkDto SameAs(string workDesc) => new()
    {
        WorkDesc = workDesc, UserCode = "T", OriginalBudget = "", RevisedBudget = "", ProposedBudget = "", Remarks = "",
    };

    private async Task<int> WorkCountAsync(int divisionId)
    {
        await using var ctx = _w.NewContext();
        return await ctx.Works.CountAsync(x => x.DivisionId == divisionId);
    }

    private async Task<int> WorkCodeAsync(int workId)
    {
        await using var ctx = _w.NewContext();
        return await ctx.Works.Where(x => x.Id == workId).Select(x => x.WorkCode).SingleAsync();
    }

    private async Task ExecAsItCellAsync(string sql)
    {
        await using var ctx = _w.NewContext();
        await using var tx = await new BudgetActionScope(ctx, _w.ItCell).BeginAsync("TEST");
        await ctx.Database.ExecuteSqlRawAsync(sql);
        await tx.CommitAsync();
    }

    /// <summary>Freezes the first cycle and opens a later, non-first cycle (2027/2028).</summary>
    private async Task MoveToNonFirstCycleAsync()
    {
        await ExecAsItCellAsync($"UPDATE budget.budget_cycle SET status = 'FROZEN' WHERE cycle_id = {_w.CycleId}");
        await ExecAsItCellAsync(
            "INSERT INTO budget.budget_cycle (current_fin_year, next_fin_year, is_first_cycle, status) VALUES (2027, 2028, false, 'OPEN')");
    }

    /// <summary>
    /// Plants an old (pre-DDD) work row of Division A. The trigger trg_work_pre_ddd_one_way refuses such an
    /// INSERT by design (only the P1 backfill marks rows), so triggers are bypassed for this one seeding
    /// statement (session_replication_role, test data only; disclosed as in P1-03).
    /// </summary>
    private async Task<int> InsertPreDddWorkAsync(int code)
    {
        var s = _w.Segments;
        await using var conn = new NpgsqlConnection(_w.ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SET session_replication_role = replica; " +
            "INSERT INTO budget.work (work_code, division_id, work_desc, major_acc_code, sub_major_acc_code, minor_acc_code, " +
            "sub_minor_acc_code, detail_acc_code, object_acc_code, original_budget, revised_budget, proposed_budget, remarks, " +
            $"user_code, pre_ddd) VALUES ({code}, {_w.HierarchyA.DivisionId}, 'Old work', '{s.Major}', '{s.SubMajor}', '{s.Minor}', " +
            $"'{s.SubMinor}', '{s.Detail}', '{s.Object}', '', '', '', '', 'T', true) RETURNING id", conn);
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await using var reset = new NpgsqlCommand("SET session_replication_role = origin", conn);
        await reset.ExecuteNonQueryAsync();
        return id;
    }

    private async Task AddDemandLineAsync(int workId)
    {
        var (c, tx) = await _w.BeginAsync(_w.DivisionA, "DEMAND_ENTER");
        await using var _ = c;
        await using var cmd = new NpgsqlCommand(
            "INSERT INTO budget.demand_line (proposal_id, line_type, serial_no, work_id, account_code_id, rbe_amount, obe_next_amount) " +
            $"VALUES ({_w.WorksProposalAId}, 'WORK', 1, {workId}, {_w.AccountCodeId}, 100, 50)", c, tx);
        await cmd.ExecuteNonQueryAsync();
        await tx.CommitAsync();
    }
}
