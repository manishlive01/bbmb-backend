using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.Update
{
    public class UpdateSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/Update-SubMinorHead", async (
                UpdateSubMinorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateSubMinorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update Sub Minor Head")
            .WithDescription("Updates an existing Sub Minor Head")
            .WithName("Update Sub Minor Head")
            .WithTags("SubMinorHead");
        }
    }
}
