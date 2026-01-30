using BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById
{
    public class GetHeadOfAccountByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/head-of-accounts/{id:int}", async (int id, IMediator mediator) =>
            {
                try
                {
                    var query = new GetHeadOfAccountByIdQuery(id);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Head of account retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "head_of_account_not_found", message = ex.Message });
                }
            })
            .WithName("GetHeadOfAccountById")
            .WithTags("HeadOfAccounts")
            .Produces<GetHeadOfAccountByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
