using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-SubMajorHead", async (
                CreateSubMajorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new CreateSubMajorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Sub Major Head")
            .WithDescription("Creates a new Sub Major Head under a Major Head")
            .WithName("Create Sub Major Head")
            .WithTags("SubMajorHead");
        }
    }
}
