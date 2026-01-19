using BBMB.BudgetService.Features.Budgets.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Budgets.Update
{
    public class UpdateBudgetEntryEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budgets/{id:int}", async (int id, UpdateBudgetEntryDto dto, IMediator mediator) =>
            {
                var command = new UpdateBudgetEntryCommand(id, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateBudgetEntry")
            .WithTags("BudgetEntries")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
