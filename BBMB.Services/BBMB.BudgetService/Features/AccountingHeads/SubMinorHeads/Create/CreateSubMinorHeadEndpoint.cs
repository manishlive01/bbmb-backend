using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Create
{
    public class CreateSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-SubMinorHead", async (
                CreateSubMinorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new CreateSubMinorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sub Minor Head")
            .WithDescription("Creates a new Sub Minor Head under a Minor Head")
            .WithName("Create Sub Minor Head")
            .WithTags("SubMinorHead");
        }
    }
}
