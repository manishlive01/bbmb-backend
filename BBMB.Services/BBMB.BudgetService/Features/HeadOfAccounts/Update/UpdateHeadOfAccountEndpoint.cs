using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public class UpdateHeadOfAccountEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/head-of-accounts/{id:int}", async (int id, UpdateHeadOfAccountDto dto, IMediator mediator) =>
            {
                var command = new UpdateHeadOfAccountCommand(id, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateHeadOfAccount")
            .WithTags("HeadOfAccounts")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
