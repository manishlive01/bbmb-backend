using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetListByUser
{
    public class GetEmployeeChargeByUserEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/employee-charges/by-user",
            async (
                HttpContext context,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetEmployeeChargeByUserQuery(context.User));

                return Results.Ok(result);
            })
            .WithName("GetEmployeeChargeByLoggedInUser")
            .WithTags("Employee Charges")
            .WithSummary("Get Employee Charges by Logged-in User")
            .WithDescription("Returns employee charge details based on logged-in user token")
            // Without this the endpoint stays reachable anonymously and, because the handler reads
            // context.User, an unauthenticated call silently returns 200 [] instead of a 401. That is
            // exactly what the server did while token validation was failing.
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}