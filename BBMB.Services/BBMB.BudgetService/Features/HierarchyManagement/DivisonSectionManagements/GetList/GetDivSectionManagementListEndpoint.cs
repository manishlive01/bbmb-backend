using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.GetList
{
    public class GetDivSectionManagementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/div-section-management", async (
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                IMediator mediator,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? divisionId = null,
                [FromQuery] int? divisionCode = null,
                [FromQuery] string? divisionName = null,
                [FromQuery] bool? status = null) =>
            {
                var query = new GetDivSectionManagementListQuery(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    sortBy,
                    sortDescending,
                    divisionId,
                    divisionCode,
                    divisionName,
                    status);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetDivSectionManagementList")
            .WithTags("DivSectionManagement")
            .Produces<PagedResponse<GetDivSectionManagementListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}