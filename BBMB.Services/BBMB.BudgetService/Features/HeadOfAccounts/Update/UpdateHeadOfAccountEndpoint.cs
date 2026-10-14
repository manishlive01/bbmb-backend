using BBMB.BudgetService.Features.HeadOfAccounts.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Update
{
    public class UpdateHeadOfAccountEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/head-of-accounts", async (
    IMediator mediator,
    [FromQuery] string majorAccCode,
    [FromQuery] string? subMajorAccCode,
    [FromQuery] string minorAccCode,
    [FromQuery] string? subMinorAccCode,
    [FromQuery] string detailAccCode,
    [FromQuery] string objectAccCode,
    UpdateHeadOfAccountDto dto) =>
            {
                var command = new UpdateHeadOfAccountCommand(
                    majorAccCode,
                    subMajorAccCode,
                    minorAccCode,
                    subMinorAccCode,
                    detailAccCode,
                    objectAccCode,
                    dto);

                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })

 .RequireAuthorization("BudgetRight:F01.Create")
 .WithName("UpdateHeadOfAccount")
 .WithTags("HeadOfAccounts")
 .Produces<Response>(StatusCodes.Status200OK)
 .Produces<Response>(StatusCodes.Status404NotFound);

        }
    }
}
