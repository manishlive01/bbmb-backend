using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public class UpdateMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/Update-MinorHead", async (
                UpdateMinorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateMinorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update Minor Head")
            .WithDescription("Updates an existing Minor Head")
            .WithName("Update Minor Head")
            .WithTags("MinorHead");
        }
    }
}
