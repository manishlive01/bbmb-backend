using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using BBMB.BudgetService.Shared.Services.EmployeeChargeServices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetList
{
    public class GetWorkListHandler : IRequestHandler<GetWorkListQuery, PagedResponse<GetWorkListResponseDto>>
    {
        private readonly BudgetContext _context;
        private readonly IChargeResolver _chargeResolver;

        public GetWorkListHandler(BudgetContext context, IChargeResolver chargeResolver)
        {
            _context = context;
            _chargeResolver = chargeResolver;
        }

        public async Task<PagedResponse<GetWorkListResponseDto>> Handle(
             GetWorkListQuery request,
             CancellationToken ct)
        {
            // ================= GET CURRENT USER CHARGE =================

            var charge = await _chargeResolver.GetCurrentChargeAsync(ct);

            if (charge == null)
                throw new Exception("User charge not found.");

            // ================= BASE QUERY =================

            var query =
                from w in _context.Works.AsNoTracking()

                join d in _context.Divisions.AsNoTracking()
                    on w.DivisionCode equals d.DivisionCode into divJoin
                from d in divJoin.DefaultIfEmpty()

                join mh in _context.MajorHeads.AsNoTracking()
                    on w.MajorAccCode equals mh.MajorAccCode into majorJoin
                from mh in majorJoin.DefaultIfEmpty()

                join smh in _context.SubMajorHeads.AsNoTracking()
                    on new { w.MajorAccCode, w.SubMajorAccCode }
                    equals new { smh.MajorAccCode, smh.SubMajorAccCode }
                    into subMajorJoin
                from smh in subMajorJoin.DefaultIfEmpty()

                join mn in _context.MinorHeads.AsNoTracking()
                    on w.MinorAccCode equals mn.MinorAccCode into minorJoin
                from mn in minorJoin.DefaultIfEmpty()

                join smn in _context.SubMinorHeads.AsNoTracking()
                    on new { w.MinorAccCode, w.SubMinorAccCode }
                    equals new { smn.MinorAccCode, smn.SubMinorAccCode }
                    into subMinorJoin
                from smn in subMinorJoin.DefaultIfEmpty()

                join dh in _context.DetailHeads.AsNoTracking()
                    on w.DetailAccCode equals dh.DetailAccCode into detailJoin
                from dh in detailJoin.DefaultIfEmpty()

                join obj in _context.ObjectClassifications.AsNoTracking()
                    on w.ObjectAccCode equals obj.ObjectAccCode into objJoin
                from obj in objJoin.DefaultIfEmpty()

                join ce in _context.CEManagements.AsNoTracking()
                    on w.CECode equals ce.CECode into ceJoin
                from ce in ceJoin.DefaultIfEmpty()

                join cr in _context.CRManagements.AsNoTracking()
                    on w.CRCode equals cr.CRCode into crJoin
                from cr in crJoin.DefaultIfEmpty()

                select new
                {
                    w,
                    d,
                    mh,
                    smh,
                    mn,
                    smn,
                    dh,
                    obj,
                    ce,
                    cr
                };

            // ================= CHARGE BASED FILTER =================

            if (charge.DivisionCode != null)
                query = query.Where(x => x.w.DivisionCode == charge.DivisionCode);

            if (charge.CECode != null)
                query = query.Where(x => x.w.CECode == charge.CECode);

            if (charge.CRCode != null)
                query = query.Where(x => x.w.CRCode == charge.CRCode);


            // ================= REQUEST FILTERS =================

            if (!string.IsNullOrWhiteSpace(request.WorkCode))
                query = query.Where(x => x.w.WorkCode.Contains(request.WorkCode));

            if (!string.IsNullOrWhiteSpace(request.WorkDesc))
                query = query.Where(x => x.w.WorkDesc.Contains(request.WorkDesc));

            if (request.DivisionCode.HasValue)
                query = query.Where(x => x.w.DivisionCode == request.DivisionCode);

            if (!string.IsNullOrWhiteSpace(request.Status))
                query = query.Where(x => x.w.Status == request.Status);

            if (!string.IsNullOrWhiteSpace(request.Nature))
                query = query.Where(x => x.w.Nature == request.Nature);

            // ================= TOTAL COUNT =================

            var totalCount = await query.CountAsync(ct);

            // ================= PAGINATION =================

            var pageSize = Math.Min(request.PageSize <= 0 ? 10 : request.PageSize, 100);

            var items = await query
                .OrderBy(x => x.w.WorkCode)
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetWorkListResponseDto
                {
                    WorkCode = x.w.WorkCode,
                    WorkDesc = x.w.WorkDesc,

                    DivisionCode = x.w.DivisionCode,
                    DivisionDesc = x.d != null ? x.d.DivisionDesc : null,
                    DivSection = x.w.DivSection,

                    MajorAccCode = x.w.MajorAccCode,
                    MajorAccDesc = x.mh != null ? x.mh.MajorAccDesc : null,

                    SubMajorAccCode = x.w.SubMajorAccCode,
                    SubMajorAccDesc = x.smh != null ? x.smh.SubMajorAccDesc : null,

                    MinorAccCode = x.w.MinorAccCode,
                    MinorAccDesc = x.mn != null ? x.mn.MinorAccDesc : null,

                    SubMinorAccCode = x.w.SubMinorAccCode,
                    SubMinorAccDesc = x.smn != null ? x.smn.SubMinorAccDesc : null,

                    DetailAccCode = x.w.DetailAccCode,
                    DetailAccDesc = x.dh != null ? x.dh.DetailAccDesc : null,

                    ObjectAccCode = x.w.ObjectAccCode,
                    ObjectAccDesc = x.obj != null ? x.obj.ObjectAccDesc : null,

                    FinYrStart = x.w.FinYrStart,
                    FinYrEnd = x.w.FinYrEnd,
                    SanctionNo = x.w.SanctionNo,
                    SanctionDate = x.w.SanctionDate,
                    DesignationCode = x.w.DesignationCode,
                    EstimatedCost = x.w.EstimatedCost,

                    Status = x.w.Status,
                    Nature = x.w.Nature,
                    RefWorkCode = x.w.RefWorkCode,

                    UserCode = x.w.UserCode,

                    CECode = x.w.CECode,
                    CEName = x.ce != null ? x.ce.Name : null,

                    CRCode = x.w.CRCode,
                    CRName = x.cr != null ? x.cr.Name : null,

                    OriginalBudget = x.w.OriginalBudget,
                    RevisedBudget = x.w.RevisedBudget,
                    ProposedBudget = x.w.ProposedBudget,
                    Remarks = x.w.Remarks,

                    AccountCode =
                        x.w.MajorAccCode + "-" +
                        x.w.SubMajorAccCode + "-" +
                        x.w.MinorAccCode + "-" +
                        x.w.SubMinorAccCode + "-" +
                        x.w.DetailAccCode + "-" +
                        x.w.ObjectAccCode
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
                    HasNextPage = request.PageNumber <
                                  (int)Math.Ceiling(totalCount / (double)pageSize)
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
