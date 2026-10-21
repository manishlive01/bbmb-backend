using BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById
{
    public class GetHeadOfAccountByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/head-of-accounts", async (
    [FromQuery] string majorAccCode,
    [FromQuery] string? subMajorAccCode,
    [FromQuery] string minorAccCode,
    [FromQuery] string? subMinorAccCode,
    [FromQuery] string detailAccCode,
    [FromQuery] string objectAccCode,
    IMediator mediator) =>
            {
                var query = new GetHeadOfAccountByIdQuery(
                    majorAccCode,
                    subMajorAccCode,
                    minorAccCode,
                    subMinorAccCode,
                    detailAccCode,
                    objectAccCode
                );

                var result = await mediator.Send(query);

                return Results.Ok(new
                {
                    message = "Head of account retrieved successfully",
                    data = result
                });
            })
.RequireAuthorization("BudgetRight:Budget.View")
.WithName("GetHeadOfAccountByKey")
.WithTags("HeadOfAccounts")
.Produces<GetHeadOfAccountByIdResponseDto>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

        }
    }
}
