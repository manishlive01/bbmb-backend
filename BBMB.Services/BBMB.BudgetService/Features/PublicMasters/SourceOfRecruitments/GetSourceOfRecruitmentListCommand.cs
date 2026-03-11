using BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.PublicMasters.SourceOfRecruitments
{
    public sealed record GetSourceOfRecruitmentListCommand()
    : IRequest<List<GetSourceOfRecruitmentListResponseDto>>;
}
