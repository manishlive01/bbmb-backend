using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetList
{
    public class GetWorkListHandler : IRequestHandler<GetWorkListQuery, PagedResponse<GetWorkListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetWorkListHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetWorkListResponseDto>> Handle(
    GetWorkListQuery request,
    CancellationToken ct)
        {
            var query = _context.Works
                .AsNoTracking()
                .AsQueryable();

            // ================= FILTERS =================

            if (!string.IsNullOrWhiteSpace(request.WorkCode))
                query = query.Where(w => w.WorkCode.Contains(request.WorkCode));

            if (!string.IsNullOrWhiteSpace(request.WorkDesc))
                query = query.Where(w => w.WorkDesc.Contains(request.WorkDesc));

            if (request.DivisionCode.HasValue)
                query = query.Where(w => w.DivisionCode == request.DivisionCode.Value);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(w => w.Status == request.Status);

            if (!string.IsNullOrWhiteSpace(request.Nature))
                query = query.Where(w => w.Nature == request.Nature);

            // ================= TOTAL COUNT =================

            var totalCount = await query.CountAsync(ct);

            // ================= SORTING =================

            query = ApplySorting(query, request.SortBy, request.SortDescending);

            // ================= PAGINATION =================

            var pageSize = Math.Min(request.PageSize <= 0 ? 10 : request.PageSize, 100);

            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new GetWorkListResponseDto
                {
                    WorkCode = w.WorkCode,
                    WorkDesc = w.WorkDesc,

                    DivisionCode = w.DivisionCode,
                    DivisionDesc = w.Division != null ? w.Division.DivisionDesc : null,

                    DivSection = w.DivSection,

                    MajorAccCode = w.MajorAccCode,
                    MajorAccDesc = w.MajorHead != null ? w.MajorHead.MajorAccDesc : null,

                    SubMajorAccCode = w.SubMajorAccCode,
                    SubMajorAccDesc = w.SubMajorHead != null ? w.SubMajorHead.SubMajorAccDesc : null,

                    MinorAccCode = w.MinorAccCode,
                    MinorAccDesc = w.MinorHead != null ? w.MinorHead.MinorAccDesc : null,

                    SubMinorAccCode = w.SubMinorAccCode,
                    SubMinorAccDesc = w.SubMinorHead != null ? w.SubMinorHead.SubMinorAccDesc : null,

                    DetailAccCode = w.DetailAccCode,
                    DetailAccDesc = w.DetailHead != null ? w.DetailHead.DetailAccDesc : null,

                    ObjectAccCode = w.ObjectAccCode,
                    ObjectAccDesc = w.ObjectClassification != null ? w.ObjectClassification.ObjectAccDesc : null,

                    FinYrStart = w.FinYrStart,
                    FinYrEnd = w.FinYrEnd,

                    SanctionNo = w.SanctionNo,
                    SanctionDate = w.SanctionDate,

                    DesignationCode = w.DesignationCode,
                    //DesignationName = w.Designation != null ? w.Designation.DesignationName : null,

                    EstimatedCost = w.EstimatedCost,

                    Status = w.Status,
                    Nature = w.Nature,
                    RefWorkCode = w.RefWorkCode,

                    UserCode = w.UserCode,
                    CRCode = w.CRCode,
                    OriginalBudget = w.OriginalBudget,
                    RevisedBudget = w.RevisedBudget,
                    ProposedBudget = w.ProposedBudget,
                    Remarks = w.Remarks,

                    AccountCode =
                        w.MajorAccCode + "-" +
                        w.SubMajorAccCode + "-" +
                        w.MinorAccCode + "-" +
                        w.SubMinorAccCode + "-" +
                        w.DetailAccCode + "-" +
                        w.ObjectAccCode
                })
                .ToListAsync(ct);

            return new PagedResponse<GetWorkListResponseDto>
            {
                Message = "Works retrieved successfully",
                Data = items,
                Pagination = new PaginationMetadata
                {
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
                }
            };
        }

        private IQueryable<BBMB.BudgetService.Features.Works.Entities.Work> ApplySorting(
            IQueryable<BBMB.BudgetService.Features.Works.Entities.Work> query, string? sortBy, bool sortDescending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(w => w.WorkCode);

            return sortBy.ToLower() switch
            {
                "workcode" => sortDescending ? query.OrderByDescending(w => w.WorkCode) : query.OrderBy(w => w.WorkCode),
                "workdesc" => sortDescending ? query.OrderByDescending(w => w.WorkDesc) : query.OrderBy(w => w.WorkDesc),
                "divisioncode" => sortDescending ? query.OrderByDescending(w => w.DivisionCode) : query.OrderBy(w => w.DivisionCode),
                _ => query.OrderBy(w => w.WorkCode)
            };
        }
    }
}
