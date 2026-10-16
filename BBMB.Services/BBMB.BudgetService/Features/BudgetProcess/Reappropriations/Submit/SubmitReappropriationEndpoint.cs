using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit
{
    public sealed class SubmitReappropriationEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/reappropriations/{id:long}/submit",
                    async (long id, SubmitReappropriationDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new SubmitReappropriationCommand(id, dto), ct)))
                .RequireAuthorization("BudgetRight:F21.Enter")
                .WithName("SubmitReappropriation")
                .WithTags("Reappropriations")
                .Produces<Response<SubmitReappropriationResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
