using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create
{
    public sealed class CreateReappropriationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reappropriations",
                    async (CreateReappropriationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new CreateReappropriationCommand(dto), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter")
                .WithName("CreateReappropriation")
                .WithTags("Reappropriations")
                .Produces<Response<long>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
