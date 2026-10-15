using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create
{
    public sealed class CreateTechnicalSanctionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/technical-sanctions",
                    async (CreateTechnicalSanctionDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new CreateTechnicalSanctionCommand(dto), ct),
                            r => Results.Created($"/api/technical-sanctions/{r.Data}", r)))
                .RequireAuthorization("BudgetRight:F06.Enter")
                .WithName("CreateTechnicalSanction")
                .WithTags("TechnicalSanctions")
                .Produces<Response<long>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
