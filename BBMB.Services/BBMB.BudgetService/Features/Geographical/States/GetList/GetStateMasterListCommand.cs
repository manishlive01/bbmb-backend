using BBMB.BudgetService.Features.Geographical.States.GetList.Dtos; 
using MediatR;

namespace BBMB.BudgetService.Features.Geographical.States.GetList
{
    public sealed record GetStateMasterListCommand() : IRequest<List<GetStateMasterListResponseDto>>;
}
