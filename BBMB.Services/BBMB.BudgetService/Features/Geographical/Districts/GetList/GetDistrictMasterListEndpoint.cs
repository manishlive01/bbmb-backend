using BBMB.BudgetService.Features.Geographical.Districts.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.Districts.GetList
{
    public class GetDistrictMasterListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-District-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetDistrictMasterListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetDistrictMasterListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get District List")
            .WithDescription("Returns list of Districts directly without response wrapper")
            .WithTags("DistrictMaster");
        }
    }
}
