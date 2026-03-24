using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.GetList.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

public sealed record GetCRSectionManagementListQuery(
    int PageNumber,
    int PageSize,
    string? SortBy,
    bool SortDescending,
    int? CRId,
    int? CRCode,       
    string? CRName,    
    bool? Status
) : IRequest<PagedResponse<GetCRSectionManagementListResponseDto>>;