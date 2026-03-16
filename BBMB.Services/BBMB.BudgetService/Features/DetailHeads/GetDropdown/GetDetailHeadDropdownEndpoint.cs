using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.DetailHeads.GetDropdown
{
    public class GetDetailHeadDropdownEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/detail-heads/dropdown", async (
                [FromQuery] string? detailAccCode,
                [FromQuery] string? detailAccDesc,
                ISender sender) =>
            {
                var query = new GetDetailHeadDropdownQuery(
                    detailAccCode,
                    detailAccDesc
                );

                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDetailHeadDropdown")
            .WithTags("DetailHeads")
            .Produces(200);
        }
    }
}