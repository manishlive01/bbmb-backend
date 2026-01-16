using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-MinorHead", async (
                CreateMinorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new CreateMinorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Minor Head")
            .WithDescription("Creates a new Minor Head")
            .WithName("Create Minor Head")
            .WithTags("MinorHead");
        }
    }
}
