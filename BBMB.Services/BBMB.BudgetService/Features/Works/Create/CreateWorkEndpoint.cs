using BBMB.BudgetService.Features.Works.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Create
{
    public class CreateWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/works", async (CreateWorkDto dto, IMediator mediator) =>
            {
                var command = new CreateWorkCommand(dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Created($"/api/works/{dto.WorkCode}", response)
                    : Results.Conflict(response);
            })
            .WithName("CreateWork")
            .WithTags("Works")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status409Conflict)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
