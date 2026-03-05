using BBMB.BudgetService.Features.EmployeeCharges.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using System.Security.Claims;

namespace BBMB.BudgetService.Features.EmployeeCharges.Create
{
    public class CreateEmployeeChargeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/employee-charges", async (
                CreateEmployeeChargeDto dto,
                HttpContext httpContext,
                IMediator mediator) =>
            {
                 
                var command = new CreateEmployeeChargeCommand(dto);

                var result = await mediator.Send(command);

                if (!result.IsSucced)
                    return Results.Conflict(result);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Employee Charges")
            .WithSummary("Create employee charge");
        }
    }
}
