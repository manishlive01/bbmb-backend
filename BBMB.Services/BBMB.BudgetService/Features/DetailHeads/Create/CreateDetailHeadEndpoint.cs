using BBMB.BudgetService.Features.DetailHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.DetailHeads.Create
{
    public class CreateDetailHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/detail-heads", async ([FromBody] CreateDetailHeadDto dto, ISender sender) =>
            {
                var command = new CreateDetailHeadCommand(dto);
                var response = await sender.Send(command);

                return response.IsSucced
                    ? Results.Created($"/api/detail-heads/{dto.DetailAccCode}", response)
                    : Results.Conflict(response);
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("CreateDetailHead")
            .WithTags("DetailHeads")
            .Produces(201)
            .Produces(400)
            .Produces(409);
        }
    }
}
