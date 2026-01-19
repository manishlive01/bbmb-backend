using BBMB.BudgetService.Features.Estimates.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.Create
{
    public class CreateEstimateEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/estimates", async (CreateEstimateDto dto, IMediator mediator) =>
            {
                var command = new CreateEstimateCommand(dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Created($"/api/estimates/{dto.WorkCode}/{dto.EstimateCode}", response)
                    : Results.Conflict(response);
            })
            .WithName("CreateEstimate")
            .WithTags("Estimates")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
