using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/works", async (CreateWorkDto dto, IMediator mediator) =>
            {
                var command = new CreateWorkCommand(dto);
                var response = await mediator.Send(command);

                // Design §4.9: M-20 → 403, other refusals → 409.
                // A work may not have a work code until budget allocation (URS WKB-004 / SRS BUD-232).
                // Do not construct the Location header from a client supplied or empty work code.
                return BudgetResults.From(response, r => Results.Created($"/api/works/{r.Data}", r));
            })
            .RequireAuthorization("BudgetRight:Works.Enter")
            .WithName("CreateWork")
            .WithTags("Works")
            .Produces<Response<int>>(StatusCodes.Status201Created)
            .Produces<Response<int>>(StatusCodes.Status403Forbidden)
            .Produces<Response<int>>(StatusCodes.Status409Conflict)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
