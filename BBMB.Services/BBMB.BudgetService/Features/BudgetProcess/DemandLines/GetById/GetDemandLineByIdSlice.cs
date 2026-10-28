using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Features.BudgetProcess.ReadRequests;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Endpoints;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetById
{
    /// <summary>One V-04 row of a visible proposal, in the list's row shape.</summary>
    public sealed record GetDemandLineByIdQuery(long ProposalId, long DemandLineId) : IRequest<Response<DemandLineRowDto>>;
    /// <summary>
    /// Same visibility as the list (proposal visible to the actor's office, else NOT_FOUND, BUD-031). A line that
    /// is not part of the proposal is NOT_FOUND. A marked-deleted line IS returned, with IsDeleted = true: the
    /// caller asked for it by id, which is the list's includeDeleted = true case (BUD-319).
    /// </summary>
    public sealed class GetDemandLineByIdHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetDemandLineByIdQuery, Response<DemandLineRowDto>>
    {
        public async Task<Response<DemandLineRowDto>> Handle(GetDemandLineByIdQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<DemandLineRowDto>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<DemandLineRowDto>(BudgetMessages.NotFound);
            var row = await context.DemandLineDisplay.AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProposalId == p.ProposalId && r.DemandLineId == request.DemandLineId, ct);
            if (row is null)
                return BudgetResponses.Fail<DemandLineRowDto>(BudgetMessages.NotFound);
            return new Response<DemandLineRowDto>
            {
                IsSucced = true, Message = "Demand line retrieved successfully",
                Data = GetDemandLinesByProposalHandler.ToRow(row),
            };
        }
    }
    public sealed class GetDemandLineByIdEndpoint : IEndpoint
    {
        public void Map(IEndpointRouteBuilder app)
        {
            // Read via POST: proposalId and lineId travel in the body, not the URL (security requirement).
            app.MapPost("/api/budget-proposals/demand-lines/detail", async (
                    DemandLineDetailRequestDto dto, IMediator mediator, CancellationToken ct) =>
                    BudgetResults.Ok(await mediator.Send(new GetDemandLineByIdQuery(dto.ProposalId, dto.LineId), ct)))
                .RequireAuthorization("BudgetRight:Budget.View")
                .WithName("GetDemandLineById")
                .WithTags("BudgetDemandLines")
                .Produces<Response<DemandLineRowDto>>(StatusCodes.Status200OK)
                .Produces<Response>(StatusCodes.Status403Forbidden)
                .Produces<Response>(StatusCodes.Status404NotFound)
                .Produces<ValidationErrorResponse>(StatusCodes.Status400BadRequest);
        }
    }
}
