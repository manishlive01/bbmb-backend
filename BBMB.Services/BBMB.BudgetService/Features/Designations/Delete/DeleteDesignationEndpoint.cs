using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public class DeleteDesignationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/designations",
                async (GetByStringIdRequestDto dto, IMediator mediator, CancellationToken ct) =>
                {
                    var response = await mediator.Send(
                        new DeleteDesignationCommand(dto), ct);

                    return response.IsSucced
                        ? Results.Ok(response)
                        : Results.BadRequest(response);
                })
            .WithName("DeleteDesignation")
            .WithTags("Designations")
            .WithSummary("Delete a designation")
            .WithDescription("Deletes a designation using string identifier from request body")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
