using BBMB.BudgetService.Features.BudgetProcess.Offices.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.Services.BudgetOffices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.GetList
{
    /// <summary>
    /// Design §8.1/§8.2: paged T-13 offices, filters level and parentId, limited to the offices visible from the
    /// actor's office (BUD-031; FINANCE and SECRETARIAT see all). No actor sees none (fail closed).
    /// </summary>
    public sealed class GetBudgetOfficeListHandler(BudgetContext context, IBudgetActorSource actorSource, IBudgetOfficeScope officeScope)
        : IRequestHandler<GetBudgetOfficeListQuery, PagedResponse<GetBudgetOfficeListResponseDto>>
    {
        public async Task<PagedResponse<GetBudgetOfficeListResponseDto>> Handle(GetBudgetOfficeListQuery request, CancellationToken ct)
        {
            var actor = await actorSource.GetAsync(ct);
            var visible = actor is null ? [] : (await officeScope.GetVisibleOfficeIdsAsync(actor.OfficeId, ct)).ToList();

            var query = context.BudgetOffices.AsNoTracking().Where(o => visible.Contains(o.OfficeId));
            if (!string.IsNullOrWhiteSpace(request.Level))
                query = query.Where(o => o.OfficeLevel == request.Level);
            if (request.ParentId is { } parent)
                query = query.Where(o => o.ParentOfficeId == parent);

            var pageNumber = Math.Max(request.PageNumber, 1);
            var pageSize = Math.Min(request.PageSize <= 0 ? 10 : request.PageSize, 100);
            var totalCount = await query.CountAsync(ct);
            var items = await query.OrderBy(o => o.OfficeId)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(o => new GetBudgetOfficeListResponseDto
                {
                    OfficeId = o.OfficeId, OfficeLevel = o.OfficeLevel, DivisionId = o.DivisionId, CircleId = o.CircleId,
                    CeId = o.CeId, WingCode = o.WingCode, ParentOfficeId = o.ParentOfficeId, IsActive = o.IsActive,
                })
                .ToListAsync(ct);
            var names = await BudgetOfficeNames.ResolveAsync(context, items.Select(i => i.OfficeId), ct);
            foreach (var i in items)
                i.OfficeName = names.GetValueOrDefault(i.OfficeId);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new PagedResponse<GetBudgetOfficeListResponseDto>
            {
                Message = "Offices retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalCount, PageNumber = pageNumber, PageSize = pageSize, TotalPages = totalPages,
                    HasPreviousPage = pageNumber > 1, HasNextPage = pageNumber < totalPages,
                },
            };
        }
    }
}
