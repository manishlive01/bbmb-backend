using BBMB.BudgetService.Shared.Endpoints;
using MediatR;
using System.Security.Claims;

namespace BBMB.BudgetService.Features.Employees.GetById
{
    public class GetEmployeeByUserIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/employees/me", async (
                HttpContext context,
                IMediator mediator) =>
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var result = await mediator.Send(
                    new GetEmployeeByUserIdQuery(Guid.Parse(userId)));

                if (result == null)
                    return Results.NotFound("Employee not found");

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithTags("Employees")
            .WithSummary("Get employee profile by logged-in user");
        }
    }
}
