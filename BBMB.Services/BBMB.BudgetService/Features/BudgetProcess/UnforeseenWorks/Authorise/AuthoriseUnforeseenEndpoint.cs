using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise
{
    public sealed class AuthoriseUnforeseenEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/unforeseen-requests/{id:long}/authorise",
                    async (long id, AuthoriseUnforeseenDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new AuthoriseUnforeseenCommand(id, dto), ct)))
                .RequireAuthorization("BudgetRight:F30.Authorise")
                .WithName("AuthoriseUnforeseen")
                .WithTags("UnforeseenWorks")
                .Produces<Response<AuthoriseUnforeseenResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
