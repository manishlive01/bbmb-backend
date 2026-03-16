using BBMB.BudgetService.Features.DetailHeads.GetDropdown.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.GetDropdown
{
    public sealed record GetDetailHeadDropdownQuery(
        string? DetailAccCode = null,
        string? DetailAccDesc = null
    ) : IRequest<List<DetailHeadDropdownDto>>;
}