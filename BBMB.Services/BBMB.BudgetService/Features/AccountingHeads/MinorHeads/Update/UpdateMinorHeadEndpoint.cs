using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Update
{
    public class UpdateMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/minor-heads/{code}", async (string code, UpdateMinorHeadDto dto, IMediator mediator) =>
            {
                var command = new UpdateMinorHeadCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("UpdateMinorHead")
            .WithTags("MinorHeads")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}
