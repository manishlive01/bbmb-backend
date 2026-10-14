using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BBMB.BudgetService.Tests.Shared.Endpoints;

public sealed class BudgetResultsTests
{
    private static int? Status(IResult r) => (r as IStatusCodeHttpResult)?.StatusCode;

    [Fact]
    public void BUD030_M20_maps_to_403()
    {
        Status(BudgetResults.Ok(BudgetResponses.Fail(BudgetMessages.M20))).Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void BUD031_NOT_FOUND_maps_to_404()
    {
        Status(BudgetResults.Ok(BudgetResponses.Fail(BudgetMessages.NotFound))).Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void BUD235_M19_maps_to_409()
    {
        Status(BudgetResults.Ok(BudgetResponses.Fail(BudgetMessages.M19))).Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Failure_without_message_id_maps_to_409()
    {
        Status(BudgetResults.Ok(new Response { IsSucced = false, Message = "x" })).Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Success_returns_the_onSuccess_result()
    {
        var ok = new Response<int> { IsSucced = true, Data = 7 };
        var result = BudgetResults.From(ok, r => Results.Created("/x", r));
        result.Should().BeOfType<Created<Response<int>>>();
        Status(result).Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public void Failure_carries_id_and_catalogue_text()
    {
        var r = BudgetResponses.Fail(BudgetMessages.M06);
        r.MessageId.Should().Be("M-06");
        r.Message.Should().StartWith("M-06: The technical sanctions");
    }
    [Fact]
    public void BUD096_rule_failures_returned_as_base_Response_keep_Data_in_the_409_body()
    {
        Response refusal = BudgetResponses.RuleFailures(new RuleFailuresDto
        {
            Failures = [new RuleFailureItemDto(1, 1, "workId", BudgetMessages.M21, "x")],
        });
        var result = BudgetResults.Ok(refusal);
        Status(result).Should().Be(StatusCodes.Status409Conflict);
        var body = (result as IValueHttpResult)!.Value;
        body.Should().BeOfType<Response<RuleFailuresDto>>("the runtime type is serialised, not the declared base type");
        System.Text.Json.JsonSerializer.Serialize(body).Should().Contain("\"Failures\"");
    }
}
