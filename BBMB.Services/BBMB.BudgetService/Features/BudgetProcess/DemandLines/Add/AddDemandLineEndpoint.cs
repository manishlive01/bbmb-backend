using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Add
{
    public sealed class AddDemandLineEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Two right keys (F07.Enter / F11.Enter, by category): Budget.View here, the key in the handler (design §8.1).
            app.MapPost("/api/budget-proposals/{id:long}/demand-lines",
                    async (long id, AddDemandLineDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new AddDemandLineCommand(id, dto), ct),
                            r => Results.Created($"/api/budget-proposals/{id}/demand-lines", (object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("AddDemandLine")
                .WithTags("BudgetDemandLines")
                .Produces<Response<SaveDemandLineResponseDto>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response<RuleFailuresDto>>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
