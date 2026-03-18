using BBMB.BudgetService.Features.Offices.OfficeManagements.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update
{
    public class UpdateOfficeManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/office-management", async (
                UpdateOfficeManagementDto dto,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                var command = new UpdateOfficeManagementCommand(dto);

                var result = await mediator.Send(command);

                if (!result.IsSucced)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
            .WithTags("Office Management")
            .WithSummary("Update office management");
        }
    }
}