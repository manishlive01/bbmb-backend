using BBMB_Budget.Features.Budgets.BudgetTypes.Create.Dtos;
using BBMB_Budget.Shared.Endpoints;
using MediatR;

namespace BBMB_Budget.Features.Budgets.BudgetTypes.Create
{
    public class CreateBudgetTypeMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Create-Budget-Type-Master", async (
                CreateBudgetTypeMasterDto dto,
                IMediator mediator) =>
            {
                var command = new CreateBudgetTypeMasterCommand(dto);

                var response = await mediator.Send(command);
                if (response.IsSucced == false)
                {
                    return Results.BadRequest(response);
                }
                return Results.Ok(response);
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithSummary("Create Budget Type Master")
            .WithDescription("Creates a new Budget Type Master entry")
            .WithName("Create Budget Type Master")
            .WithTags("BudgetTypeMaster");
        }
    }
}
