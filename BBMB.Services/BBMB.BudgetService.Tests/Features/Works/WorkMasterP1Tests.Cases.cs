using System.Text.Json;
using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Tests.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Tests.Features.Works;

public sealed partial class WorkMasterP1Tests
{
    [Fact]
    public async Task BUD052_Works_CreateInFirstCycle_copies_the_six_segments_and_sets_LEGACY_OPENING()
    {
        var r = await CreateAsync(_w, _w.DivisionA, NewWorkDto());
        r.IsSucced.Should().BeTrue(r.Message);

        await using var ctx = _w.NewContext();
        var work = await ctx.Works.AsNoTracking().SingleAsync(x => x.WorkDesc == "New continuing work");
        work.DivisionId.Should().Be(_w.HierarchyA.DivisionId);
        work.MajorAccCode.Should().Be(_w.Segments.Major);
        work.ObjectAccCode.Should().Be(_w.Segments.Object);
        work.EnteredVia.Should().Be(WorkEnteredVia.LegacyOpening);
        work.PreDdd.Should().BeFalse();
    }

    [Fact]
    public async Task BUD052_Works_CreateInAnotherDivisionRefusedM20()
    {
        var before = await WorkCountAsync(_w.HierarchyB.DivisionId);
        (await CreateAsync(_w, _w.DivisionA, NewWorkDto(divisionId: _w.HierarchyB.DivisionId))).MessageId.Should().Be("M-20");
        (await WorkCountAsync(_w.HierarchyB.DivisionId)).Should().Be(before, "nothing is written");
    }

    [Fact]
    public async Task S52_Works_Create_with_unknown_or_not_for_use_account_code_gets_M05()
    {
        (await CreateAsync(_w, _w.DivisionA, NewWorkDto(accountCodeId: 987654))).MessageId.Should().Be("M-05");

        await ExecAsItCellAsync(
            $"UPDATE budget.budget_head_master SET not_for_use = true, not_for_use_ref = 'T/1', not_for_use_at = now(), " +
            $"not_for_use_by = '{_w.ItCell.Actor!.EmployeeId}' WHERE account_code_id = {_w.AccountCodeId}");
        (await CreateAsync(_w, _w.DivisionA, NewWorkDto())).MessageId.Should().Be("M-05");
    }

    [Fact]
    public async Task L08_Works_Create_when_the_first_cycle_is_not_OPEN_gets_M19()
    {
        await MoveToNonFirstCycleAsync();
        var before = await WorkCountAsync(_w.HierarchyA.DivisionId);
        (await CreateAsync(_w, _w.DivisionA, NewWorkDto())).MessageId.Should().Be("M-19", "a later OPEN cycle does not make the guard pass");
        (await WorkCountAsync(_w.HierarchyA.DivisionId)).Should().Be(before);
    }

    [Fact]
    public async Task L08_Works_Create_after_the_Division_first_submitted_gets_M19()
    {
        await using (var ctx = _w.NewContext())
            (await TestServices.Transitions(ctx, _w.DivisionA).MoveAsync(_w.WorksProposalAId, "SUBMIT", null)).IsSucced.Should().BeTrue();
        (await CreateAsync(_w, _w.DivisionA, NewWorkDto())).MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task L06_Works_Create_before_any_T14_row_gets_M19()
    {
        var empty = await SpineWorld.CreateAsync(fixture, $"wm0_{Guid.NewGuid():N}"[..20], withCycle: false);
        var dto = NewWorkDto(divisionId: empty.HierarchyA.DivisionId, accountCodeId: empty.AccountCodeId);
        (await CreateAsync(empty, empty.DivisionA, dto)).MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task S93_Works_Create_body_carrying_enteredVia_AA_ENTRY_stores_LEGACY_OPENING()
    {
        var json = JsonSerializer.Serialize(NewWorkDto(), JsonSerializerOptions.Web)
            .TrimEnd('}') + ",\"enteredVia\":\"AA_ENTRY\",\"preDdd\":true}";
        var dto = JsonSerializer.Deserialize<CreateWorkDto>(json, JsonSerializerOptions.Web)!;

        (await CreateAsync(_w, _w.DivisionA, dto)).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        var work = await ctx.Works.AsNoTracking().SingleAsync(x => x.WorkDesc == "New continuing work");
        work.EnteredVia.Should().Be(WorkEnteredVia.LegacyOpening);
        work.PreDdd.Should().BeFalse();
    }

    [Fact]
    public async Task BUD031_Works_Update_of_another_Division_work_is_NOT_FOUND()
    {
        var codeB = await WorkCodeAsync(_w.WorkBId);
        (await UpdateAsync(codeB, SameAs("Renamed"))).MessageId.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task S113_Works_RenameInNonFirstCycleSucceeds()
    {
        await MoveToNonFirstCycleAsync();
        var r = await UpdateAsync(await WorkCodeAsync(_w.WorkAId), SameAs("Renamed work"));
        r.IsSucced.Should().BeTrue(r.Message);

        await using var ctx = _w.NewContext();
        var changes = await ctx.AuditLog.AsNoTracking()
            .Where(a => a.TableName == "work" && a.RecordKey == _w.WorkAId.ToString() && a.Operation == "U" && a.ActionCode == "WORKS_UPDATE")
            .Select(a => a.ColumnName).ToListAsync();
        changes.Count(c => c == "work_desc").Should().Be(1, "one T-51 row for the rename");
        changes.Should().NotContain(new[] { "work_class", "major_acc_code", "division_id", "entered_via", "pre_ddd" });
    }

    [Fact]
    public async Task S113_Works_ReclassifyReferencedByDemandLineRefusedM19()
    {
        await AddDemandLineAsync(_w.WorkAId);
        var code = await WorkCodeAsync(_w.WorkAId);

        var reclass = SameAs("Work 500001");
        reclass.WorkClass = "AMR";
        (await UpdateAsync(code, reclass)).MessageId.Should().Be("M-19");

        var rehead = SameAs("Work 500001");
        rehead.AccountCodeId = 987654; // refused by the reference rule before the code lookup (design §5.2)
        (await UpdateAsync(code, rehead)).MessageId.Should().Be("M-19");

        await using var ctx = _w.NewContext();
        var work = await ctx.Works.AsNoTracking().SingleAsync(x => x.Id == _w.WorkAId);
        work.WorkClass.Should().Be("AA");
        work.MajorAccCode.Should().Be(_w.Segments.Major);
    }

    [Fact(Skip = "Runnable when T-25/T-26 land: P2-07")]
    public async Task S113_Works_ReclassifyWithT26VersionRefusedM19()
    {
        // P2-07: create an administrative approval (T-25) with a version (T-26) on work A, then reclassify.
        // WorkRules.IsReferencedAsync is extended with T-25/T-27 in that change (design §11.3, §12, §14).
        var reclass = SameAs("Work 500001");
        reclass.WorkClass = "AMR";
        (await UpdateAsync(await WorkCodeAsync(_w.WorkAId), reclass)).MessageId.Should().Be("M-19");
    }

    [Fact]
    public async Task S113_Works_Reclassify_unreferenced_work_succeeds()
    {
        var reclass = SameAs("Work 500001");
        reclass.WorkClass = "AMR";
        (await UpdateAsync(await WorkCodeAsync(_w.WorkAId), reclass)).IsSucced.Should().BeTrue();
        await using var ctx = _w.NewContext();
        (await ctx.Works.AsNoTracking().SingleAsync(x => x.Id == _w.WorkAId)).WorkClass.Should().Be("AMR");
    }

    [Fact]
    public async Task L08_Works_Clearing_pre_ddd_in_first_cycle_sets_LEGACY_OPENING()
    {
        var id = await InsertPreDddWorkAsync(590001);
        var dto = SameAs("Old work");
        dto.WorkClass = "AA";
        var r = await UpdateAsync(590001, dto);
        r.IsSucced.Should().BeTrue(r.Message);

        await using var ctx = _w.NewContext();
        var work = await ctx.Works.AsNoTracking().SingleAsync(x => x.Id == id);
        work.PreDdd.Should().BeFalse();
        work.WorkClass.Should().Be("AA");
        work.EnteredVia.Should().Be(WorkEnteredVia.LegacyOpening);
        work.CreatedBy.Should().BeNull("an old row's creation stays unknown");
    }

    [Fact]
    public async Task L08_Works_Clearing_pre_ddd_outside_first_cycle_gets_M19()
    {
        var id = await InsertPreDddWorkAsync(590002);
        await MoveToNonFirstCycleAsync();
        var dto = SameAs("Old work");
        dto.WorkClass = "AA";
        (await UpdateAsync(590002, dto)).MessageId.Should().Be("M-19");

        await using var ctx = _w.NewContext();
        (await ctx.Works.AsNoTracking().SingleAsync(x => x.Id == id)).PreDdd.Should().BeTrue();
    }
}
