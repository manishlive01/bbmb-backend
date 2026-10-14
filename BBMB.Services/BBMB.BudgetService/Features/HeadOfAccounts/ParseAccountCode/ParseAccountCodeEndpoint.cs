using BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BBMB.BudgetService.Features.HeadOfAccounts.ParseAccountCode
{
    public class ParseAccountCodeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/head-of-accounts/parse/{accountCode}", async (string accountCode, IMediator mediator) =>
            {
                try
                {
                    var query = new ParseAccountCodeQuery(accountCode);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Account code parsed successfully", data = result });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = "invalid_account_code", message = ex.Message });
                }
            })
            .RequireAuthorization("BudgetRight:Budget.View")
            .WithName("ParseAccountCode")
            .WithTags("HeadOfAccounts")
            .Produces<ParseAccountCodeResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        }
    }
}
