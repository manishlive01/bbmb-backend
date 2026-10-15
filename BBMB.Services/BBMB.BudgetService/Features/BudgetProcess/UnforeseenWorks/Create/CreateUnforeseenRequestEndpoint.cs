using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create
{
    public sealed class CreateUnforeseenRequestEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/unforeseen-requests",
                    async (CreateUnforeseenRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new CreateUnforeseenRequestCommand(dto), ct)))
                .RequireAuthorization("BudgetRight:F30.Submit")
                .WithName("CreateUnforeseenRequest")
                .WithTags("UnforeseenWorks")
                .Produces<Response<long>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
