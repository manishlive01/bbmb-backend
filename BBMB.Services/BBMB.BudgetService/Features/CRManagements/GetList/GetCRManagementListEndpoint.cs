using BBMB.BudgetService.Features.CRManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.CRManagements.GetList
{
    public class GetCRManagementListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/cr-management", async (
                [FromQuery] int? pageNumber,
                [FromQuery] int? pageSize,
                IMediator mediator,
                [FromQuery] string? sortBy = null,
                [FromQuery] bool sortDescending = false,
                [FromQuery] int? crCode = null,
                [FromQuery] int? ceCode = null,
                [FromQuery] bool? status = null) =>
            {
                var query = new GetCRManagementListQuery(
                    pageNumber ?? 1,
                    pageSize ?? 10,
                    sortBy,
                    sortDescending,
                    crCode,
                    ceCode,
                    status);

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCRManagementList")
            .WithTags("CRManagement")
            .Produces<PagedResponse<GetCRManagementListResponseDto>>(StatusCodes.Status200OK);

            app.MapPost("/api/List-By-CRCode", async ( 
                IMediator mediator,
                [FromBody] GetByIdRequestDto requestDto
                 ) =>
            {
                var query = new GetCRManagementListById(requestDto
                    );

                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetCRManagementListById")
            .WithDescription("Get CRManagement list by Circle code")
            .WithTags("CRManagement")
            .Produces<List<GetCRManagementListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
