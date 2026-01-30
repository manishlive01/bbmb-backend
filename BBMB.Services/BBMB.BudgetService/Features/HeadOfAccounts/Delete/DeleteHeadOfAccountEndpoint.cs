using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Delete
{
    public class DeleteHeadOfAccountEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/head-of-accounts/{id:int}", async (int id, IMediator mediator) =>
            {
                var command = new DeleteHeadOfAccountCommand(id);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return response.Message.Contains("not found")
                        ? Results.NotFound(response)
                        : Results.Conflict(response);
                }

                return Results.NoContent();
            })
            .WithName("DeleteHeadOfAccount")
            .WithTags("HeadOfAccounts")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
