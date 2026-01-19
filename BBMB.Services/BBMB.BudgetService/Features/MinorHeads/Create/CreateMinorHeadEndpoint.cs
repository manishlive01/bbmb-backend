using BBMB.BudgetService.Features.MinorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.Create
{
    public class CreateMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/minor-heads", async (CreateMinorHeadDto dto, IMediator mediator, ILogger<CreateMinorHeadEndpoint> logger) =>
            {
                try
                {
                    var command = new CreateMinorHeadCommand(dto);
                    var response = await mediator.Send(command);

                    return response.IsSucced
                        ? Results.Created($"/api/minor-heads/{dto.MinorAccCode}", response)
                        : Results.BadRequest(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating minor head with code {MinorAccCode}", dto.MinorAccCode);
                    throw; // Re-throw to let global exception handler deal with it
                }
            })
            .WithName("CreateMinorHead")
            .WithTags("MinorHeads")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError);
        }
    }
}
