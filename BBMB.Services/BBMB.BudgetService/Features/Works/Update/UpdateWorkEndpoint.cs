using BBMB.BudgetService.Features.Works.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.Update
{
    public class UpdateWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/works/{code}", async (int code, UpdateWorkDto dto, IMediator mediator) =>
            {
                var command = new UpdateWorkCommand(code, dto);
                var response = await mediator.Send(command);

                return response.IsSucced
                    ? Results.Ok(response)
                    : Results.NotFound(response);
            })
            .RequireAuthorization("BudgetRight:Works.Enter")
            .WithName("UpdateWork")
            .WithTags("Works")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces<Response>(StatusCodes.Status404NotFound)
            .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
