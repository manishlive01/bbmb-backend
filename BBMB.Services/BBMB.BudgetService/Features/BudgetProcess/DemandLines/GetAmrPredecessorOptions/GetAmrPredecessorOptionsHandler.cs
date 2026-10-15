using BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Proposals;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetAmrPredecessorOptions
{
    /// <summary>
    /// Design §8.3 amr-predecessor-options (BUD-275): the AM&amp;R works of the same Division with a non-deleted
    /// line in the preceding cycle's WORKS proposal. Scoped (NOT_FOUND outside it). Empty for a head-wise
    /// proposal and where no preceding cycle exists (the first cycle).
    /// </summary>
    public sealed class GetAmrPredecessorOptionsHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetAmrPredecessorOptionsQuery, Response<List<AmrPredecessorOptionDto>>>
    {
        public async Task<Response<List<AmrPredecessorOptionDto>>> Handle(GetAmrPredecessorOptionsQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            if (actor is null)
                return BudgetResponses.Fail<List<AmrPredecessorOptionDto>>(BudgetMessages.M20);
            var p = await ProposalReads.FindVisibleAsync(context, officeScope, actor, request.ProposalId, false, ct);
            if (p is null)
                return BudgetResponses.Fail<List<AmrPredecessorOptionDto>>(BudgetMessages.NotFound);
            var works = await DemandLineWrites.AmrPredecessorsAsync(context, p, ct);
            return new Response<List<AmrPredecessorOptionDto>>
            {
                IsSucced = true, Message = "AM&R predecessor options retrieved successfully",
                Data = works.Select(w => new AmrPredecessorOptionDto(w.Id, w.WorkCode, w.WorkDesc)).ToList(),
            };
        }
    }
}
