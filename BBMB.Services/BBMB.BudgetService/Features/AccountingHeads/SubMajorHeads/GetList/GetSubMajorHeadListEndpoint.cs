using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public class GetSubMajorHeadListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/Get-SubMajorHead-List", async (
                IMediator mediator) =>
            {
                var response = await mediator.Send(
                    new GetSubMajorHeadListCommand());

                return Results.Ok(response);
            })
            .Produces<List<GetSubMajorHeadListResponseDto>>(StatusCodes.Status200OK)
            .WithSummary("Get Sub Major Head List")
            .WithDescription("Gets the list of all Sub Major Heads")
            .WithName("Get Sub Major Head List")
            .WithTags("SubMajorHead");
        }
    }
}
