using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public class CreateMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-MajorHead", async (
                CreateMajorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new CreateMajorHeadCommand(dto);

                var response = await mediator.Send(command);

                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Major Head")
            .WithDescription("Creates a new Major Head entry")
            .WithName("Create Major Head")
            .WithTags("MajorHead");
        }
    }
}
