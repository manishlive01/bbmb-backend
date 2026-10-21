using BBMB.BudgetService.Features.BudgetProcess.Proposals.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Return.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.Return
{
    public sealed class ReturnProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-proposals/{id:long}/return", async (long id, ReturnProposalDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new ReturnProposalCommand(id, dto), ct)))
                .RequireAuthorization("BudgetRight:Review.Examine")
                .WithName("ReturnProposal")
                .WithTags("BudgetProposals")
                .Produces<Response<ProposalStateResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
