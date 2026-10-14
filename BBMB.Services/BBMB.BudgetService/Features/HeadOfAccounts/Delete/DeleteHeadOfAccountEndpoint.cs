using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Delete
{
    public class DeleteHeadOfAccountEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/head-of-accounts", async (
     [FromQuery] string majorAccCode,
     [FromQuery] string? subMajorAccCode,
     [FromQuery] string minorAccCode,
     [FromQuery] string? subMinorAccCode,
     [FromQuery] string detailAccCode,
     [FromQuery] string objectAccCode,
     IMediator mediator) =>
            {
                var command = new DeleteHeadOfAccountCommand(
                    majorAccCode,
                    subMajorAccCode,
                    minorAccCode,
                    subMinorAccCode,
                    detailAccCode,
                    objectAccCode
                );

                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                        ? Results.NotFound(response)
                        : Results.Conflict(response);
                }

                return Results.NoContent();
            })
 .RequireAuthorization("BudgetRight:F01.Create")
 .WithName("DeleteHeadOfAccount")
 .WithTags("HeadOfAccounts")
 .Produces(StatusCodes.Status204NoContent)
 .Produces<Response>(StatusCodes.Status404NotFound)
 .Produces<Response>(StatusCodes.Status409Conflict);

        }
    }
}
