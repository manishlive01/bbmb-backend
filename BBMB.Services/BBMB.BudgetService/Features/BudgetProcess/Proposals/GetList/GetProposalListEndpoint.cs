using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList
{
    public sealed class GetProposalListEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals", async (
                    IMediator mediator, CancellationToken ct,
                    [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] long? cycleId = null,
                    [FromQuery] string? category = null, [FromQuery] string? state = null, [FromQuery] long? officeId = null,
                    [FromQuery] bool pendingWithMe = false) =>
                    Results.Ok(await mediator.Send(
                        new GetProposalListQuery(pageNumber, pageSize, cycleId, category, state, officeId, pendingWithMe), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetProposalList")
                .WithTags("BudgetProposals")
                .Produces<PagedResponse<GetProposalListResponseDto>>(StatusCodes.Status200OK);
        }
    }
}
