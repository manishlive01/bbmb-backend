using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;
using System.Security.Claims;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public class GetEmployeeByUserIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/employees/byId", async (
                HttpContext context,
                GetByStringIdRequestDto dto,
                IMediator mediator) =>
            {
                 
                var result = await mediator.Send(
                    new GetEmployeeByUserIdQuery(Guid.Parse(dto.Id)));

                if (result == null)
                    return Results.NotFound("Employee not found");

                return Results.Ok(result);
            })
            .WithTags("Employees")
            .WithSummary("Get employee profile by logged-in user");
        }
    }
}
