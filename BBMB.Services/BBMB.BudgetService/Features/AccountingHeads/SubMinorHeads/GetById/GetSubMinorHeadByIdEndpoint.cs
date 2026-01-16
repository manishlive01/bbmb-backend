using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById
{
    public class GetSubMinorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-SubMinorHead-ById", async (
                GetByStringIdRequestDto request,
                IMediator mediator) =>
            {
                var command = new GetSubMinorHeadByIdCommand(request);

                var response = await mediator.Send(command);
                if (response == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(response);
            })
            .Produces<GetSubMinorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get Sub Minor Head By Id")
            .WithDescription("Gets Sub Minor Head details by Sub Minor Account Code")
            .WithName("Get Sub Minor Head By Id")
            .WithTags("SubMinorHead");
        }
    }
}
