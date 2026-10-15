using BBMB.BudgetService.Features.BudgetProcess.Offices.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.GetList
{
    public sealed class GetBudgetOfficeListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-offices", async (
                    IMediator mediator,
                    CancellationToken ct,
                    [FromQuery] int pageNumber = 1,
                    [FromQuery] int pageSize = 10,
                    [FromQuery] string? level = null,
                    [FromQuery] long? parentId = null) =>
                    Results.Ok(await mediator.Send(new GetBudgetOfficeListQuery(pageNumber, pageSize, level, parentId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetBudgetOfficeList")
                .WithTags("BudgetOffices")
                .Produces<PagedResponse<GetBudgetOfficeListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
