using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById
{
    public class GetMinorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-MinorHead-ById", async (
                GetByStringIdRequestDto request,
                IMediator mediator) =>
            {
                var command = new GetMinorHeadByIdCommand(request);

                var response = await mediator.Send(command);
                if (response == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(response);
            })
            .Produces<GetMinorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get Minor Head By Id")
            .WithDescription("Gets Minor Head details by Minor Account Code")
            .WithName("Get Minor Head By Id")
            .WithTags("MinorHead");
        }
    }
}
