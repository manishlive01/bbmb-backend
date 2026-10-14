using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Create
{
    public class CreateMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/minor-heads", async (CreateMinorHeadDto dto, IMediator mediator, ILogger<CreateMinorHeadEndpoint> logger) =>
            {
                
                    var command = new CreateMinorHeadCommand(dto);
                    var response = await mediator.Send(command);

                    return response.IsSucced
                        ? Results.Created($"/api/minor-heads/{dto.MinorAccCode}", response)
                        : Results.BadRequest(response);
               
            })
            .RequireAuthorization("BudgetRight:F01.Create")
            .WithName("CreateMinorHead")
            .WithTags("MinorHeads")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError);
        }
    }
}
