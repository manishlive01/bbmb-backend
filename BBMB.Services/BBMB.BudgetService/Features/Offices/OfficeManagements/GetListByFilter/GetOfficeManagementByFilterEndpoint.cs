using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetListByFilter
{
    public class GetOfficeManagementByFilterEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/office-management/filter", async (
                IMediator mediator,
                [FromQuery] int? ceCode,
                [FromQuery] int? crCode,
                [FromQuery] int? divisionCode) =>
            {
                var result = await mediator.Send(
                    new GetOfficeManagementByFilterQuery(ceCode, crCode, divisionCode));

                return Results.Ok(result);
            })
            .WithTags("Office Management")
            .WithSummary("Get office management by CE/CR/Division");
        }
    }
}