using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public class UpdateMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/Update-MajorHead", async (
                UpdateMajorHeadDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateMajorHeadCommand(dto);

                var response = await mediator.Send(command);

                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }

                return Results.Ok(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update Major Head")
            .WithDescription("Updates an existing Major Head entry")
            .WithTags("MajorHead");
        }
    }
}
