using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList
{
    public class GetCRSectionManagementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cr-section-management", async (
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                IMediator mediator,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? crId = null,
                [FromQuery] int? crCode = null,
                [FromQuery] string? crName = null,
                [FromQuery] bool? status = null) =>
            {
                var query = new GetCRSectionManagementListQuery(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    sortBy,
                    sortDescending,
                    crId,
                    crCode,
                    crName,
                    status);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCRSectionManagementList")
            .WithTags("CRSectionManagement")
            .Produces<PagedResponse<GetCRSectionManagementListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}