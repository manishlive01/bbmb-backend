using BBMB_Budget.Features.Budgets.BudgetTypes.Update.Dtos;
using BBMB_Budget.Shared.Endpoints;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Update
{
    public class UpdateBudgetTypeMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Update-Budget-Type-Master", async (
                UpdateBudgetTypeMasterDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateBudgetTypeMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Update Budget Type Master")
            .WithDescription("Updates a new Budget Type Master entry")
            .WithName("Update Budget Type Master")
            .WithTags("BudgetTypeMaster");
        }
    }
}
