using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById
{
    public sealed class GetProposalByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId travels in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/detail", async (ProposalReadRequestDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetProposalByIdQuery(dto.ProposalId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetProposalById")
                .WithTags("BudgetProposals")
                .Produces<Response<GetProposalByIdResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
