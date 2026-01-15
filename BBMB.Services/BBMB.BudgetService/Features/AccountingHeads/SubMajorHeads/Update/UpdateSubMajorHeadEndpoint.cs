using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public class UpdateSubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/Update-SubMajorHead", async (
                UpdateSubMajorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateSubMajorHeadCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update Sub Major Head")
            .WithDescription("Updates an existing Sub Major Head")
            .WithName("Update Sub Major Head")
            .WithTags("SubMajorHead");
        }
    }
}
