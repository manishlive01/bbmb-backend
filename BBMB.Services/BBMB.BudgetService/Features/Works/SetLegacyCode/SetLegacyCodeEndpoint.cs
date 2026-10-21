using BBMB.BudgetService.Features.Works.SetLegacyCode.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.SetLegacyCode
{
    public sealed class SetLegacyCodeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/works/{id:int}/legacy-code", async (int id, SetLegacyCodeDto dto, IMediator mediator) =>
            {
                var command = new SetLegacyCodeCommand(id, dto);
                var response = await mediator.Send(command);

                // Design §4.9: NOT_FOUND → 404 (another Division's work is not disclosed), M-20 → 403, else 409.
                return BudgetResults.Ok(response);
            })
            .RequireAuthorization("BudgetRight:Works.Enter")
            .WithName("SetWorkLegacyCode")
            .WithTags("Works")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<Response>(StatusCodes.Status409Conflict)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
