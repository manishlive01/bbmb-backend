using BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.EstimateAllocations.GetForDivision
{
    public sealed class GetForDivisionEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/estimate-allocations",
                    async (int divisionId, int finYear, IMediator mediator, CancellationToken ct) =>
                        Results.Ok(await mediator.Send(new GetForDivisionQuery(divisionId, finYear), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetEstimateAllocationsForDivision")
                .WithTags("EstimateAllocations")
                .Produces<List<EstimateAllocationWorkDto>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status403Forbidden);
        }
    }
}
