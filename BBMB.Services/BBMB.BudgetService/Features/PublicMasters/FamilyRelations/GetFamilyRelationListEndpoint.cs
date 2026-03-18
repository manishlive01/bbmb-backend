using BBMB.BudgetService.Features.PublicMasters.FamilyRelations.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.FamilyRelations
{
    public class GetFamilyRelationListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-FamilyRelation-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetFamilyRelationListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetFamilyRelationListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Family Relation List")
            .WithTags("Public");
        }
    }
}