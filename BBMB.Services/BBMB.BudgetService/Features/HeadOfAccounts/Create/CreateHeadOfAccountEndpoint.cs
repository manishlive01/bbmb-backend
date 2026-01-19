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
            app.MapPost("/api/head-of-accounts", async (CreateHeadOfAccountDto dto, IMediator mediator) =>
            {
                var command = new CreateHeadOfAccountCommand(dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Created($"/api/head-of-accounts/{dto.MajorAccCode}", response)
                    : Results.Conflict(response);
            })
            .WithName("CreateHeadOfAccount")
            .WithTags("HeadOfAccounts")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
