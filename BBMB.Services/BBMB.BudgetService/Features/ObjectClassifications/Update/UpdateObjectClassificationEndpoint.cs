using BBMB.BudgetService.Features.ObjectClassifications.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Update
{
    public class UpdateObjectClassificationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/object-classifications/{code}", async (string code, UpdateObjectClassificationDto dto, IMediator mediator) =>
            {
                var command = new UpdateObjectClassificationCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("UpdateObjectClassification")
            .WithTags("ObjectClassifications")
            .WithSummary("Update an object classification")
            .WithDescription("Updates an existing object classification's description and user code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
