using BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.GetByProposal
{
    /// <summary>Design §8.3: every observation of a visible proposal, oldest first (BUD-121). Scoped (NOT_FOUND outside it).</summary>
    public sealed class GetObservationsByProposalHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetObservationsByProposalQuery, Response<List<ObservationResponseDto>>>
    {
        public async Task<Response<List<ObservationResponseDto>>> Handle(GetObservationsByProposalQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<ObservationResponseDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<List<ObservationResponseDto>>(BudgetMessages.NotFound);
            var items = await (
                from o in context.Observations.AsNoTracking()
                where o.ProposalId == p.ProposalId
                join l in context.DemandLines.AsNoTracking() on o.DemandLineId equals (long?)l.DemandLineId into lj
                from l in lj.DefaultIfEmpty()
                orderby o.CreatedAt, o.ObservationId
                select new ObservationResponseDto
                {
                    ObservationId = o.ObservationId, DemandLineId = o.DemandLineId, SerialNo = l == null ? null : l.SerialNo,
                    LevelOfficeId = o.LevelOfficeId, ObservationText = o.ObservationText, RecordedBy = o.CreatedBy,
                    RecordedAt = o.CreatedAt, SelectedForReturn = o.SelectedForReturn, ReturnedByMovementId = o.ReturnedByMovementId,
                    ReplyText = o.ReplyText, RepliedBy = o.RepliedBy, RepliedAt = o.RepliedAt,
                }).ToListAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, items.Select(i => i.LevelOfficeId), ct);
            foreach (var i in items)
                i.LevelOfficeName = names.GetValueOrDefault(i.LevelOfficeId);
            return new Response<List<ObservationResponseDto>> { IsSucced = true, Message = "Observations retrieved successfully", Data = items };
        }
    }
}
