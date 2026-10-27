using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions
{
    public sealed class GetAmrPredecessorOptionsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId travels in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/amr-predecessor-options",
                    async (ProposalReadRequestDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new GetAmrPredecessorOptionsQuery(dto.ProposalId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetAmrPredecessorOptions")
                .WithTags("BudgetDemandLines")
                .Produces<Response<List<AmrPredecessorOptionDto>>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
