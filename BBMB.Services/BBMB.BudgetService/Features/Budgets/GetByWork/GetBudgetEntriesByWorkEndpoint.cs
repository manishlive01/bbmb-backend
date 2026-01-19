using BBMB.BudgetService.Features.Budgets.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.GetByWork
{
    public class GetBudgetEntriesByWorkEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budgets/by-work/{workCode}", async (
                string workCode,
                [FromQuery] int pageNumber,
                [FromQuery] int pageSize,
                IMediator mediator) =>
            {
                var query = new GetBudgetEntriesByWorkQuery(workCode, pageNumber, pageSize);
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetBudgetEntriesByWork")
            .WithTags("BudgetEntries")
            .Produces<PagedResponse<GetBudgetEntryListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
