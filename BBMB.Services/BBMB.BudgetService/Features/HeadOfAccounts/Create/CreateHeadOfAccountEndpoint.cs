using BBMB.BudgetService.Features.HeadOfAccounts.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.HeadOfAccounts.Create
{
    public class CreateHeadOfAccountEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/head-of-accounts", async (CreateHeadOfAccountDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.From(await mediator.Send(new CreateHeadOfAccountCommand(dto), ct),
                        r => Results.Created("/api/head-of-accounts", r)))
                .RequireAuthorization("BudgetRight:F01.Create")
                .WithName("CreateHeadOfAccount")
                .WithTags("HeadOfAccounts")
                .Produces<Response<CreateAccountCodeResponseDto>>(StatusCodes.Status201Created)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
