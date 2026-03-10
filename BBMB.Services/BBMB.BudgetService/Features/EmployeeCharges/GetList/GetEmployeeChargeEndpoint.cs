using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.EmployeeCharges.GetList
{
    public class GetEmployeeChargeEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/employee-charges", async (
                IMediator mediator) =>
            {

                var result = await mediator.Send(new GetEmployeeChargeQuery());

                return Results.Ok(result);
            })
            .WithName("GetEmployeeChargeByUser")
            .WithTags("Employee Charges")
            .WithSummary("Get Employee Charges by Logged-in User")
            .WithDescription("Returns employee charge details based on logged-in user token")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}