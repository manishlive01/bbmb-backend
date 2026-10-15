using BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.SetWing
{
    public sealed class SetOfficeWingEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/budget-offices/{id:long}/wing", async (long id, SetOfficeWingDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new SetOfficeWingCommand(id, dto), ct)))
                .RequireAuthorization("BudgetRight:Offices.Load")
                .WithName("SetOfficeWing")
                .WithTags("BudgetOffices")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
