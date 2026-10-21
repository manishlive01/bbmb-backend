using BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals.Dtos;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Cycles.OpenProposals
{
    public sealed class OpenProposalsEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/budget-cycles/{cycleId:long}/open-proposals",
                    async (long cycleId, OpenProposalsDto dto, IMediator mediator, CancellationToken ct) =>
                        BudgetResults.Ok(await mediator.Send(new OpenProposalsCommand(cycleId, dto), ct)))
                .RequireAuthorization("BudgetRight:Cycle.Manage")
                .WithName("OpenProposals")
                .WithTags("BudgetCycles")
                .Produces<Response<OpenProposalsResponseDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<Response>(StatusCodes.Status409Conflict)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
