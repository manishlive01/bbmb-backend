using BBMB.BudgetService.Features.Works.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Works.GetByDivision
{
    public class GetWorksByDivisionHandler : IRequestHandler<GetWorksByDivisionQuery, PagedResponse<GetWorkListResponseDto>>
    {
        private readonly BudgetContext _context;

        public GetWorksByDivisionHandler(BudgetContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetWorkListResponseDto>> Handle(GetWorksByDivisionQuery request, CancellationToken ct)
        {
            var divisionExists = await _context.Divisions.FirstOrDefaultAsync(d => d.DivisionCode == request.DivisionCode, ct);
            var query = _context.Works
                .Include(w => w.Division)
                .Where(w => w.DivisionId == divisionExists.Id);

            var totalCount = await query.CountAsync(ct);

            var pageSize = Math.Min(request.PageSize, 100);
            var items = await query
                .Skip((request.PageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(w => new GetWorkListResponseDto
                {
                    //WorkCode = w.WorkCode,
                    WorkDesc = w.WorkDesc,
                    //DivisionCode = w.DivisionCode,
                    DivisionDesc = w.Division != null ? w.Division.DivisionDesc : null,
                    AccountCode = $"{w.MajorAccCode}-{w.SubMajorAccCode}-{w.MinorAccCode}-{w.SubMinorAccCode}-{w.DetailAccCode}-{w.ObjectAccCode}",
                    Status = w.Status,
                    Nature = w.Nature,
                    UserCode = w.UserCode
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
    }
}
