using BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag
{
    public sealed class SetPartyEmpFlagEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/head-of-accounts/{accountCodeId:long}/party-emp-flag",
                    async (long accountCodeId, SetPartyEmpFlagDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new SetPartyEmpFlagCommand(accountCodeId, dto), ct)))
                .RequireAuthorization("BudgetRight:F01.FlagPartyEmp")
                .WithName("SetAccountCodePartyEmpFlag")
                .WithTags("HeadOfAccounts")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
