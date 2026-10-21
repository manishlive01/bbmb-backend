using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList.Dtos;
using BBMB.BudgetService.Features.BudgetProcess.Offices;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetList
{
    /// <summary>
    /// Design §8.3: proposals whose origin or holder is visible from the actor's office (BUD-031); filters
    /// cycleId, category, state, officeId (origin) and pendingWithMe (held by the actor's office). No actor sees none.
    /// </summary>
    public sealed class GetProposalListHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetProposalListQuery, PagedResponse<GetProposalListResponseDto>>
    {
        public async Task<PagedResponse<GetProposalListResponseDto>> Handle(GetProposalListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            var visible = actor is null ? [] : (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToList();
            var query = context.BudgetProposals.AsNoTracking()
                .Where(p => visible.Contains(p.OriginOfficeId) || visible.Contains(p.HolderOfficeId));
            if (request.CycleId is { } cycle) query = query.Where(p => p.CycleId == cycle);
            if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(p => p.Category == request.Category);
            if (!string.IsNullOrWhiteSpace(request.State)) query = query.Where(p => p.StateCode == request.State);
            if (request.OfficeId is { } office) query = query.Where(p => p.OriginOfficeId == office);
            if (request.PendingWithMe && actor is not null) query = query.Where(p => p.HolderOfficeId == actor.OfficeId);

            var pageNumber = Math.Max(request.PageNumber, 1);
            var pageSize = Math.Min(request.PageSize <= 0 ? 10 : request.PageSize, 100);
            var total = await query.CountAsync(ct);
            var items = await (
                from p in query
                join s in context.ProposalStates on p.StateCode equals s.StateCode
                orderby p.ProposalId
                select new GetProposalListResponseDto
                {
                    ProposalId = p.ProposalId, CycleId = p.CycleId, Category = p.Category, OriginOfficeId = p.OriginOfficeId,
                    StateCode = p.StateCode, StateName = s.StateName, HolderOfficeId = p.HolderOfficeId,
                    FirstSubmittedAt = p.FirstSubmittedAt,
                }).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, items.SelectMany(i => new[] { i.OriginOfficeId, i.HolderOfficeId }), ct);
            foreach (var i in items)
            {
                i.OriginOfficeName = names.GetValueOrDefault(i.OriginOfficeId);
                i.HolderOfficeName = names.GetValueOrDefault(i.HolderOfficeId);
            }
            var pages = (int)Math.Ceiling(total / (double)pageSize);
            return new PagedResponse<GetProposalListResponseDto>
            {
                Message = "Proposals retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = total, PageNumber = pageNumber, PageSize = pageSize, TotalPages = pages,
                    HasPreviousPage = pageNumber > 1, HasNextPage = pageNumber < pages,
                },
            };
        }
    }
}
