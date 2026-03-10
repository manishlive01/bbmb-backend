using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.Delete
{
    public class DeleteEmployeeChargeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/employee-charges/{id}", async (
                Guid id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteEmployeeChargeCommand(id));

                if (!result.IsSucced)
                    return Results.BadRequest(result);

                return Results.Ok(result);
            })
            .WithName("DeleteEmployeeCharge")
            .WithTags("Employee Charges")
            .WithSummary("Delete Employee Charge")
            .WithDescription("Deletes employee charge record by Id");
        }
    }
}