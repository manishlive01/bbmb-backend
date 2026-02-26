using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GenerateWorkCodes
{
    public class GenerateWorkCodeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/works/generate-code/{divisionCode:int}",
                async (int divisionCode, IMediator mediator) =>
                {
                    var response = await mediator.Send(
                        new GenerateWorkCodeQuery(divisionCode));

                    return response.IsSucced
                        ? Results.Ok(response)
                        : Results.BadRequest(response);
                })
                .WithName("GenerateWorkCode")
                .WithTags("Works")
                .Produces<Response>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status400BadRequest);
        }
    }
}