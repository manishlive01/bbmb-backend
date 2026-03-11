using BBMB.BudgetService.Features.Designations.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Designations
{
    public class GetDesignationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-Designation-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetDesignationListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetDesignationListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Designation List")
            .WithTags("Public");
        }
    }
}