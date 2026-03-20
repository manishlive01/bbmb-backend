using BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR
{
    public sealed record GetDivisionByCECRQuery(
        int ceId,
        int crId
    ) : IRequest<List<GetDivisionByCECRResponseDto>>;
}