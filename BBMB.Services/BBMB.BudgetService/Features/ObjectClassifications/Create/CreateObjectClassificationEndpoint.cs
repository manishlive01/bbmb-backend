using BBMB.BudgetService.Features.ObjectClassifications.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Create
{
    public class CreateObjectClassificationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/object-classifications", async (CreateObjectClassificationDto dto, IMediator mediator) =>
            {
                var command = new CreateObjectClassificationCommand(dto);
                var response = await mediator.Send(command);

                if (!response.IsSucced)
                {
                    return Results.Conflict(response);
                }

                return Results.Created($"/api/object-classifications/{dto.ObjectAccCode}", response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("CreateObjectClassification")
            .WithTags("ObjectClassifications")
            .WithSummary("Create a new object classification")
            .WithDescription("Creates a new object classification entry")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);
        }
    }
}
