using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Create.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.GetList.Dtos;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.GetList
{
    public class GetLevelMangementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/level", async ( 
   IMediator mediator ) =>
            {
                var query = new GetLevelManagementListQuery(
                    );

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
                .RequireAuthorization()
//.RequireAuthorization("Budget:Admin")
.WithName("GetLevelManagementList")
.WithTags("LevelManagement")
.Produces<PagedResponse<GetLevelListResponseDto>>(StatusCodes.Status200OK);
        }
    }

}
