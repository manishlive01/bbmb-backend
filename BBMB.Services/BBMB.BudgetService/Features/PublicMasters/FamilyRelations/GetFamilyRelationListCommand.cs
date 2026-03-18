using BBMB.BudgetService.Features.PublicMasters.FamilyRelations.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.FamilyRelations
{
    public sealed record GetFamilyRelationListCommand()
        : IRequest<List<GetFamilyRelationListResponseDto>>;
}