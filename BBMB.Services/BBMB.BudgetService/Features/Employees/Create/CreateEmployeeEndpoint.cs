using BBMB.BudgetService.Features.Divisions.Create.Dtos;
using BBMB.BudgetService.Features.Employees.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Employees.Create
{
    public class CreateEmployeeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/employees", async ([FromForm] CreateEmployeeDto dto, IMediator mediator) =>
            {
                var command = new CreateEmployeeCommand(dto);

                var response = await mediator.Send(command);

                if (!response.IsSucced)
                    return Results.Conflict(response);

                return Results.Created("/api/employees", response);
            })
            .WithName("CreateEmployee")
            .DisableAntiforgery()
            .WithTags("Employees")
            .WithSummary("Create employee")
            .WithDescription("Creates employee with employment, photo and family details")
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict).Accepts<CreateEmployeeDto>("multipart/form-data");
        }
    }
}
