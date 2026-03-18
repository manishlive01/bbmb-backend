using BBMB.BudgetService.Features.Offices.OfficeManagements.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.OfficeManagements.Create
{
    public class CreateOfficeManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/office-management", async (
                CreateOfficeManagementDto dto,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var command = new CreateOfficeManagementCommand(dto);

                var result = await mediator.Send(command);

                if (!result.IsSucced)
                    return Results.Conflict(result);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Office Management")
            .WithSummary("Create office management");
        }
    }
}