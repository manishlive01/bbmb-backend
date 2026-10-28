using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal
{
    public sealed class GetDemandLinesByProposalEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId, tab and paging travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/demand-lines", async (
                    DemandLinesListRequestDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(
                        new GetDemandLinesByProposalQuery(dto.ProposalId, dto.Tab, dto.IncludeDeleted, dto.PageNumber, dto.PageSize), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDemandLinesByProposal")
                .WithTags("BudgetDemandLines")
                .Produces<Response<GetDemandLinesResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
