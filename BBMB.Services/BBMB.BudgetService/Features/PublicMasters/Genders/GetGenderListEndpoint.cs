using BBMB.BudgetService.Features.PublicMasters.Genders.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Genders
{
    public class GetGenderListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-Gender-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetGenderListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetGenderListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Gender List")
            .WithTags("Public");
        }
    }
}
