using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById
{
    public class GetMajorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-MajorHead-ById", async (
                [FromBody] GetByStringIdRequestDto dto,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var result = await mediator.Send(
                    new GetMajorHeadByIdCommand(dto), ct);

                return result == null
                    ? Results.NotFound("Major Head not found")
                    : Results.Ok(result);
            })
            .Produces<GetMajorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get Major Head By Id")
            .WithDescription("Returns major head details using a universal GetById request DTO")
            .WithTags("MajorHead");
        }
    }
}
