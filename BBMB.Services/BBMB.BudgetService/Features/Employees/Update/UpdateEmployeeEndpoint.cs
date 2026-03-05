using BBMB.BudgetService.Features.Employees.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Employees.Update
{
    public class UpdateEmployeeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPut("/api/employees", async (
                [FromForm] CreateEmployeeDto dto,
                IMediator mediator) =>
            {
                var command = new UpdateEmployeeCommand(dto);

                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.NotFound(response);

                return Results.Ok(response);
            })
            .DisableAntiforgery()
            .RequireAuthorization()
            .WithTags("Employees")
            .WithSummary("Update employee")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}
