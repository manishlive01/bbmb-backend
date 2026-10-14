using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.Delete
{
    public class DeleteDetailHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/detail-heads/{code}", async (string code, ISender sender) =>
            {
                var command = new DeleteDetailHeadCommand(code);
                var response = await sender.Send(command);

                if (!response.IsSucced)
                {
                    if (response.Message.Contains("not found"))
                        return Results.NotFound(response);
                    else
                        return Results.Conflict(response);
                }

                return Results.NoContent();
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("DeleteDetailHead")
            .WithTags("DetailHeads")
            .Produces(204)
            .Produces(404)
            .Produces(409);
        }
    }
}
