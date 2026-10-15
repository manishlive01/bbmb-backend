using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetById
{
    public sealed class GetProposalByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/budget-proposals/{id:long}", async (long id, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetProposalByIdQuery(id), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetProposalById")
                .WithTags("BudgetProposals")
                .Produces<Response<GetProposalByIdResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status404NotFound);
        }
    }
}
