using BBMB.BudgetService.Features.Designations.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public class UpdateDesignationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/designations/update", async (
                UpdateDesignationDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var response = await mediator.Send(
                    new UpdateDesignationCommand(dto), ct);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .WithName("UpdateDesignation")
            .WithTags("Designations")
            .WithSummary("Update a designation")
            .WithDescription("Updates an existing designation using request body")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
