using BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HierarchyManagement.Divisions.GetByCECR
{
    public sealed record GetDivisionByCECRQuery(
        int CECode,
        int CRCode
    ) : IRequest<List<GetDivisionByCECRResponseDto>>;
}