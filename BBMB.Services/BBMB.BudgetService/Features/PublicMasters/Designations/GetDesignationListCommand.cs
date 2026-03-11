using BBMB.BudgetService.Features.PublicMasters.Designations.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.Designations
{
    public sealed record GetDesignationListCommand()
    : IRequest<List<GetDesignationListResponseDto>>;
}