using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetFigureHistory
{
    /// <summary>
    /// Design §8.3 figure-history: every T-31 version of a line of a visible proposal, oldest first (the order
    /// they were written). Scoped (NOT_FOUND outside it, BUD-031); a marked-deleted line keeps its history.
    /// </summary>
    public sealed class GetFigureHistoryHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetFigureHistoryQuery, Response<List<FigureHistoryItemDto>>>
    {
        public async Task<Response<List<FigureHistoryItemDto>>> Handle(GetFigureHistoryQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<FigureHistoryItemDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null || !await context.DemandLines.AsNoTracking()
                    .AnyAsync(l => l.DemandLineId == request.DemandLineId && l.ProposalId == p.ProposalId, ct))
                return BudgetResponses.Fail<List<FigureHistoryItemDto>>(BudgetMessages.NotFound);
            var items = await context.FigureVersions.AsNoTracking()
                .Where(v => v.DemandLineId == request.DemandLineId)
                .OrderBy(v => v.CreatedAt).ThenBy(v => v.FigureVersionId)
                .Select(v => new FigureHistoryItemDto
                {
                    FigureVersionId = v.FigureVersionId, Figure = v.Figure, Value = v.Value, LevelOfficeId = v.LevelOfficeId,
                    StateCode = v.StateCode, ChangeSource = v.ChangeSource, Justification = v.Justification,
                    ContinuedWithoutJustification = v.ContinuedWithoutJustification, RoleCode = v.RoleCode,
                    OnBehalfOfPost = v.OnBehalfOfPost, OfficerId = v.CreatedBy, ChargeId = v.CreatedChargeId, EnteredAt = v.CreatedAt,
                }).ToListAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, items.Select(i => i.LevelOfficeId), ct);
            foreach (var i in items)
                i.LevelOfficeName = names.GetValueOrDefault(i.LevelOfficeId);
            return new Response<List<FigureHistoryItemDto>> { IsSucced = true, Message = "Figure history retrieved successfully", Data = items };
        }
    }
}
