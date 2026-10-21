using BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Distributions.GetForCe
{
    public sealed class GetDistributionsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-cycles/{cycleId:long}/distributions",
                    async (long cycleId, string category, long ceOfficeId, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.From(await mediator.Send(new GetDistributionsQuery(cycleId, category, ceOfficeId), ct),
                            r => Results.Ok((object)r)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDistributionsForCe")
                .WithTags("BudgetDistributions")
                .Produces<Response<List<DistributionHeadDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden);
        }
    }
}
