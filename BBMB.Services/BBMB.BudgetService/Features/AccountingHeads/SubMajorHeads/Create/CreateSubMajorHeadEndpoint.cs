using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public class CreateSubMajorHeadEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/accounting-heads/sub-major", async (CreateSubMajorHeadDto dto, IMediator mediator) =>
            {
                var command = new CreateSubMajorHeadCommand(dto);
                var response = await mediator.Send(command);
                
                return response.IsSucced 
                    ? Results.Created($"/api/accounting-heads/sub-major/{dto.SubMajorAccCode}", response)
                    : Results.Conflict(response);
            })
            .WithName("CreateSubMajorHead")
            .WithTags("SubMajorHeads")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces<Response>(StatusCodes.Status409Conflict);
        }
    }
}
