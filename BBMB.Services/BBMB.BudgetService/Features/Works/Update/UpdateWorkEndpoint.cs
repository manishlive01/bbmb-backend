using BBMB.BudgetService.Features.Works.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Update
{
    public class UpdateWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/works/{id:int}", async (int id, UpdateWorkDto dto, IMediator mediator) =>
            {
                var command = new UpdateWorkCommand(id, dto);
                var response = await mediator.Send(command);

                // Design §4.9: NOT_FOUND → 404 (another Division's work is not disclosed), M-20 → 403, else 409.
                return BudgetResults.Ok(response);
            })
            .RequireAuthorization("BudgetRight:Works.Enter")
            .WithName("UpdateWork")
            .WithTags("Works")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status409Conflict)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
