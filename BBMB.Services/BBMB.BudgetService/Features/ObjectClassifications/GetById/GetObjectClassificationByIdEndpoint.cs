using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetById
{
    public class GetObjectClassificationByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/object-classifications/{code}", async (string code, IMediator mediator) =>
            {
                try
                {
                    var query = new GetObjectClassificationByIdQuery(code);
                    var result = await mediator.Send(query);
                    return Results.Ok(new { message = "Object classification retrieved successfully", data = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { error = "object_classification_not_found", message = ex.Message });
                }
            })
            .WithName("GetObjectClassificationById")
            .WithTags("ObjectClassifications")
            .WithSummary("Get object classification by code")
            .WithDescription("Retrieves a single object classification by its code")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
