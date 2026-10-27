using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe
{
    public sealed class GetDistributionsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: cycleId, category and ceOfficeId travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-cycles/distributions",
                    async (DistributionsRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetDistributionsQuery(dto.CycleId, dto.Category, dto.CeOfficeId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDistributionsForCe")
                .WithTags("BudgetDistributions")
                .Produces<Response<List<DistributionHeadDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
