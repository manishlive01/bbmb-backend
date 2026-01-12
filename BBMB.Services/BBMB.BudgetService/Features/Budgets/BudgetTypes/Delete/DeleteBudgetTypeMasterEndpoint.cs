using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs; 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.Delete
{
    public class DeleteBudgetTypeMasterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/Delete-Budget-Type-Master", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var response = await mediator.Send(
                    new DeleteBudgetTypeMasterCommand(dto), ct);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.BadRequest(response);
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .WithSummary("Delete Budget Type Master")
            .WithDescription("Soft deletes a Budget Type Master using BudgetTypeMasterId")
            .WithTags("BudgetTypeMaster");
        }
    }
}
