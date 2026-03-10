using BBMB.BudgetService.Features.EmployeeCharges.Update.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.EmployeeCharges.Update
{
    public class UpdateEmployeeChargeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/employee-charges", async (
                [FromBody] UpdateEmployeeChargeDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateEmployeeChargeCommand(dto);

                var result = await mediator.Send(command);

                return Results.Ok(new
                {
                    Message = "Employee charge updated successfully"
                });
            })
            .RequireAuthorization()
            .WithName("UpdateEmployeeCharge")
            .WithTags("Employee Charges")
            .WithSummary("Update Employee Charge")
            .WithDescription("Updates employee charge details")
            .Produces(StatusCodes.Status200OK);
        }
    }
}