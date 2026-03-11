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
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();
        }
    }
}