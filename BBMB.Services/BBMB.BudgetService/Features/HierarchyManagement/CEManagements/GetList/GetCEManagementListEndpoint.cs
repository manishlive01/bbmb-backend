using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList
{
    public class GetCEManagementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/ce-management", async (
     [FromQuery] int? pageNumber,
     [FromQuery] int? pageSize,
     IMediator mediator,
     [FromQuery] string? sortBy = null,
     [FromQuery] bool sortDescending = false,
     [FromQuery] string? projectCode = null,
     [FromQuery] bool? status = null) =>
            {
                var query = new GetCEManagementListQuery(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    sortBy,
                    sortDescending,
                    projectCode,
                    status);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
 //.RequireAuthorization("Budget:Admin")
 .WithName("GetCEManagementList")
 .WithTags("CEManagement")
 .Produces<PagedResponse<GetCEManagementListResponseDto>>(StatusCodes.Status200OK);

        }
    }
}
