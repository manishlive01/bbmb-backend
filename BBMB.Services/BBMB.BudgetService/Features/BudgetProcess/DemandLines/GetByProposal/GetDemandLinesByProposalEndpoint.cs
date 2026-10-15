using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal
{
    public sealed class GetDemandLinesByProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}/demand-lines", async (
                    long id, IMediator mediator, CancellationToken ct,
                    [FromQuery] string? tab = null, [FromQuery] bool includeDeleted = false,
                    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50) =>
                    BudgetResults.Ok(await mediator.Send(
                        new GetDemandLinesByProposalQuery(id, tab, includeDeleted, pageNumber, pageSize), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDemandLinesByProposal")
                .WithTags("BudgetDemandLines")
                .Produces<Response<GetDemandLinesResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
