using BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments
{
    public class GetSourceOfRecruitmentListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-SourceOfRecruitment-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetSourceOfRecruitmentListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetSourceOfRecruitmentListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Source Of Recruitment List")
            .WithTags("Public");
        }
    }
}
