using BBMB.BudgetService.Features.Designations.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public class CreateDesignationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/designations", async (CreateDesignationDto dto, IMediator mediator) =>
            {
                var command = new CreateDesignationCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/designations/{dto.DesignationCode}", response);
            })
            .WithName("CreateDesignation")
            .WithTags("Designations")
            .WithSummary("Create a new designation")
            .WithDescription("Creates a new designation entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
