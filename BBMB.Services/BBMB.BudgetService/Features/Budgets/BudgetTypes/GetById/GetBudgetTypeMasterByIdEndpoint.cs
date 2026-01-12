using BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs; 
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.Budgets.BudgetTypes.GetById
{
    public class GetBudgetTypeMasterByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-Budget-Type-Master-ById", async (
                [FromBody] GetByIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetBudgetTypeMasterByIdCommand(dto), ct);

                return result == null
                    ? Results.NotFound("BudgetTypeMaster not found")
                    : Results.Ok(result);
            })
            .Produces<GetBudgetTypeMasterByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get BudgetTypeMaster By Id")
            .WithDescription("Returns Budget Type Master details using a universal GetById request DTO")
            .WithTags("BudgetTypeMaster");
        }
    }
}
