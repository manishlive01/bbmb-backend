using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public class GetSubMajorHeadByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/Get-SubMajorHead-ById", async (
                GetByStringIdRequestDto request,
                IMediator mediator) =>
            {
                var command = new GetSubMajorHeadByIdCommand(request);

                var response = await mediator.Send(command);
                if (response == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(response);
            })
            .Produces<GetSubMajorHeadByIdResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithSummary("Get Sub Major Head By Id")
            .WithDescription("Gets a Sub Major Head by MajorAccCode and SubMajorAccCode")
            .WithName("Get Sub Major Head By Id")
            .WithTags("SubMajorHead");
        }
    }
}
