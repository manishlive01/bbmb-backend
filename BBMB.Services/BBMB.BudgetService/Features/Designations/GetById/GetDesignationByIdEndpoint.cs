using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Designations.GetById
{
    public class GetDesignationByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/designations/byId", async (
     [FromBody] GetByStringIdRequestDto dto,
     IMediator mediator,
     CancellationToken ct) =>
            {
                var query = new GetDesignationByIdQuery(dto);

                var result = await mediator.Send(query, ct);

                if (result == null)
                {
                    return Results.NotFound(new
                    {
                        error = "designation_not_found",
                        message = $"Designation with code '{dto.Id}' not found"
                    });
                }

                return Results.Ok(new
                {
                    message = "Designation retrieved successfully",
                    data = result
                });
            })
 .WithName("GetDesignationById")
 .WithTags("Designations")
 .WithSummary("Get Designation By Id")
 .WithDescription("Retrieves designation details using request body")
 .Produces(StatusCodes.Status200OK)
 .Produces(StatusCodes.Status404NotFound);
        }
    }
}
