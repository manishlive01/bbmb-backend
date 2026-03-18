using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Delete
{
    public class DeleteOfficeManagementEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/office-management/{officeCode}", async (
                string officeCode,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteOfficeManagementCommand(officeCode));

                if (!result.IsSucced)
                    return Results.NotFound(result);

                return Results.Ok(result);
            })
            .WithTags("Office Management")
            .WithSummary("Delete office management");
        }
    }
}