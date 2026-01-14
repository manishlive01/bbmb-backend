using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList
{
    public class GetMajorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-MajorHead-List", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetMajorHeadListCommand(), ct);

                return Results.Ok(result);
            })
            .Produces<List<GetMajorHeadListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Major Head List")
            .WithDescription("Returns list of major heads directly without response wrapper")
            .WithTags("MajorHead");
        }
    }
}
