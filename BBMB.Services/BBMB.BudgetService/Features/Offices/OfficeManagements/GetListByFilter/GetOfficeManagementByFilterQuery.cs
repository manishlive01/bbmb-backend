using BBMB.BudgetService.Features.Offices.OfficeManagements.GetListByFilter.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.GetListByFilter
{
    public record GetOfficeManagementByFilterQuery(
        int? CECode,
        int? CRCode,
        int? DivisionCode
    ) : IRequest<List<GetOfficeManagementByFilterDto>>;
}