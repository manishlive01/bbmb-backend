using BBMB.BudgetService.Features.PublicMasters.Titles.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Titles.GetList
{
    public sealed record GetTitleListCommand()
        : IRequest<List<GetTitleListResponseDto>>;
}