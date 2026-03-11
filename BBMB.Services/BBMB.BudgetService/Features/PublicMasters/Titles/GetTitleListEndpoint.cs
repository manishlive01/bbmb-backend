using BBMB.BudgetService.Features.PublicMasters.Titles.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Titles.GetList
{
    public class GetTitleListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-Title-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetTitleListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetTitleListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Title List")
            .WithDescription("Returns list of titles")
            .WithTags("Public");
        }
    }
}