using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinor.Create
{
    public class CreateSubMinorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/minor-heads/sub-minor", async (CreateSubMinorHeadDto dto, IMediator mediator, ILogger<CreateSubMinorHeadEndpoint> logger) =>
            {
                try
                {
                    var command = new CreateSubMinorHeadCommand(dto);
                    var response = await mediator.Send(command);

                    return response.IsSucced
                        ? Results.Created($"/api/minor-heads/sub-minor/{dto.SubMinorAccCode}", response)
                        : Results.BadRequest(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error creating sub-minor head with code {SubMinorAccCode}", dto.SubMinorAccCode);
                    throw; // Re-throw to let global exception handler deal with it
                }
            })
            .WithName("CreateSubMinorHead")
            .WithTags("SubMinorHeads")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status400BadRequest)
            .Produces<Response>(StatusCodes.Status500InternalServerError);
        }
    }
}
