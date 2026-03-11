using BBMB.BudgetService.Features.PublicMasters.Genders.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Genders
{
    public sealed record GetGenderListCommand()
     : IRequest<List<GetGenderListResponseDto>>;
}
