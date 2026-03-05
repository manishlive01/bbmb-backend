using BBMB.BudgetService.Features.Employees.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using MediatR;

namespace BBMB.BudgetService.Features.Employees.GetList
{
    public class GetEmployeeListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/employees", async (
                [AsParameters] GetEmployeeListRequestDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetEmployeeListQuery(dto));

                return Results.Ok(result);
            }).RequireAuthorization()
            .WithName("GetEmployeeList")
            .WithTags("Employees")
            .WithSummary("Get employee list with pagination")
            .WithDescription("Retrieves employees with filtering, sorting, and pagination");
        }
    }
}
