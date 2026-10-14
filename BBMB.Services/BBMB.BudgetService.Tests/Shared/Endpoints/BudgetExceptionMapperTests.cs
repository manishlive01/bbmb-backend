using System.Text.Json;
using BBMB.BudgetService.Shared.Endpoints;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BBMB.BudgetService.Tests.Shared.Endpoints;

/// <summary>Design §4.9 and §10 mapping rows.</summary>
public sealed class BudgetExceptionMapperTests
{
    private static PostgresException Pg(string state, string text, string table = "demand_line", string constraint = "fk_demand_line_work") =>
        new(text, "ERROR", "ERROR", state, tableName: table, constraintName: constraint, where: "PL/pgSQL function budget.fn_x()");

    private static DbUpdateException Wrapped(Exception inner) => new("An error occurred while saving the entity changes.", inner);

    private static void BodyHasNoInternalNames(MappedFailure m)
    {
        var json = JsonSerializer.Serialize(m.Body, m.Body.GetType());
        json.Should().NotContain("demand_line").And.NotContain("fk_demand_line_work").And.NotContain("fn_x");
    }

    [Fact]
    public void BUD012_raised_M06_inside_DbUpdateException_maps_to_409_with_catalogue_text()
    {
        var m = BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.RaiseException, "M-06")));
        m.Should().NotBeNull();
        m!.StatusCode.Should().Be(409);
        m.Body.MessageId.Should().Be("M-06");
        m.Body.Message.Should().StartWith("M-06: The technical sanctions");
        m.LogDetail.Should().Contain("fk_demand_line_work", "the log, not the body, names the constraint");
        BodyHasNoInternalNames(m);
    }

    [Fact]
    public void Raw_PostgresException_is_mapped_too()
    {
        BudgetExceptionMapper.TryMap(Pg(PostgresErrorCodes.RaiseException, "M-17"))!.Body.MessageId.Should().Be("M-17");
    }

    [Fact]
    public void BUD012_foreign_key_violation_maps_to_409_M05()
    {
        var m = BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.ForeignKeyViolation, "violates fk")));
        m!.StatusCode.Should().Be(409);
        m.Body.MessageId.Should().Be("M-05");
        BodyHasNoInternalNames(m);
    }

    [Fact]
    public void Unregistered_unique_violation_is_not_mapped()
    {
        BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.UniqueViolation, "dup", constraint: "ux_unknown"))).Should().BeNull();
    }

    [Fact]
    public void BUD015_concurrency_conflict_maps_to_409_M23()
    {
        var m = BudgetExceptionMapper.TryMap(new DbUpdateConcurrencyException("xmin"));
        m!.StatusCode.Should().Be(409);
        m.Body.MessageId.Should().Be("M-23");
    }

    [Fact]
    public void DB28_ACTOR_is_not_mapped_and_falls_through_to_500()
    {
        BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.RaiseException, "ACTOR"))).Should().BeNull();
        BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.RaiseException, "FORBIDDEN"))).Should().BeNull();
    }

    [Fact]
    public void Unknown_exceptions_are_not_mapped()
    {
        BudgetExceptionMapper.TryMap(new InvalidOperationException("x")).Should().BeNull();
        BudgetExceptionMapper.TryMap(Wrapped(Pg(PostgresErrorCodes.CheckViolation, "check"))).Should().BeNull();
        BudgetExceptionMapper.TryMap(null).Should().BeNull();
    }
}
