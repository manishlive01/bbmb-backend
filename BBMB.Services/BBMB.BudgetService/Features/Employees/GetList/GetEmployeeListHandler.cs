using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.Employees.GetList.Dtos;
using BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Features.Employees.GetList
{
    public class GetEmployeeListHandler
    : IRequestHandler<GetEmployeeListQuery, PagedResponse<GetEmployeeListResponseDto>>
    {
        private readonly EmployeeContext _context;

        public GetEmployeeListHandler(EmployeeContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<GetEmployeeListResponseDto>> Handle(
            GetEmployeeListQuery request,
            CancellationToken ct)
        {
            var dto = request.dto;

            var query = _context.Employees
                .Include(x => x.EmploymentDetail)
                .Include(x => x.PhotoDocument)
                .AsQueryable();

            // Filters
            if (!string.IsNullOrWhiteSpace(dto.NameEnglish))
                query = query.Where(x => x.NameEnglish.Contains(dto.NameEnglish));

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                query = query.Where(x => x.PhoneNumber.Contains(dto.PhoneNumber));

            if (!string.IsNullOrWhiteSpace(dto.Email))
                query = query.Where(x => x.Email.Contains(dto.Email));

            if (!string.IsNullOrWhiteSpace(dto.Gender))
                query = query.Where(x => x.Gender == dto.Gender);

            if (!string.IsNullOrWhiteSpace(dto.Designation))
                query = query.Where(x => x.EmploymentDetail.Designation.Contains(dto.Designation));

            var totalCount = await query.CountAsync(ct);

            query = ApplySorting(query, dto.SortBy, dto.SortDescending);

            var pageSize = Math.Min(dto.PageSize, 100);
            var pageNumber = Math.Max(dto.PageNumber, 1);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectToType<GetEmployeeListResponseDto>()
                .ToListAsync(ct);

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };

            return new PagedResponse<GetEmployeeListResponseDto>
            {
                Message = "Employees retrieved successfully",
                Data = items,
                Pagination = paginationMetadata
            };
        }

        private IQueryable<Employee> ApplySorting(
            IQueryable< Employee> query,
            string? sortBy,
            bool sortDescending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(x => x.NameEnglish);

            query = sortBy.ToLower() switch
            {
                "nameenglish" => sortDescending ? query.OrderByDescending(x => x.NameEnglish) : query.OrderBy(x => x.NameEnglish),
                "email" => sortDescending ? query.OrderByDescending(x => x.Email) : query.OrderBy(x => x.Email),
                "phonenumber" => sortDescending ? query.OrderByDescending(x => x.PhoneNumber) : query.OrderBy(x => x.PhoneNumber),
                "designation" => sortDescending
                    ? query.OrderByDescending(x => x.EmploymentDetail.Designation)
                    : query.OrderBy(x => x.EmploymentDetail.Designation),

                _ => query.OrderBy(x => x.NameEnglish)
            };

            return query;
        }
    }
}
