using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise
{
    public sealed class ReviseTechnicalSanctionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/technical-sanctions/{id:long}/revisions",
                    async (long id, ReviseTechnicalSanctionDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new ReviseTechnicalSanctionCommand(id, dto), ct),
                            r => Results.Created($"/api/technical-sanctions/{id}", r)))
                .RequireAuthorization("BudgetRight:F06.Enter")
                .WithName("ReviseTechnicalSanction")
                .WithTags("TechnicalSanctions")
                .Produces<Response<int>>(StatusCodes.Status201Created)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
