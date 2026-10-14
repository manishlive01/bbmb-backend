using BBMB.BudgetService.Features.DetailHeads.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.DetailHeads.Update
{
    public class UpdateDetailHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/detail-heads/{code}", async (string code, [FromBody] UpdateDetailHeadDto dto, ISender sender) =>
            {
                var command = new UpdateDetailHeadCommand(code, dto);
                var response = await sender.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("UpdateDetailHead")
            .WithTags("DetailHeads")
            .Produces(200)
            .Produces(400)
            .Produces(404);
        }
    }
}
