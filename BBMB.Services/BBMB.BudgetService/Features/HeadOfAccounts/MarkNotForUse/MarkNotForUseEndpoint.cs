using BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.HeadOfAccounts.MarkNotForUse
{
    public sealed class MarkNotForUseEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/head-of-accounts/{accountCodeId:long}/not-for-use",
                    async (long accountCodeId, MarkNotForUseDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new MarkNotForUseCommand(accountCodeId, dto), ct)))
                .RequireAuthorization("BudgetRight:F01.MarkNotForUse")
                .WithName("MarkAccountCodeNotForUse")
                .WithTags("HeadOfAccounts")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
