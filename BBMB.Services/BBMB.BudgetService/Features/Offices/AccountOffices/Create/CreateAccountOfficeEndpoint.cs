using BBMB.BudgetService.Features.Offices.AccountOffices.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.AccountOffices.Create
{
    public class CreateAccountOfficeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/account-offices", async (
                CreateAccountOfficeDto dto,
                IMediator mediator) =>
            {
                var command = new CreateAccountOfficeCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/account-offices/{dto.AccOffice}", response);
            })
            .WithName("CreateAccountOffice")
            .WithTags("AccountOffices")
            .WithSummary("Create a new account office")
            .WithDescription("Creates a new account office entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
