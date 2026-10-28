using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Create
{
    public sealed record CreateUnforeseenRequestCommand(CreateUnforeseenRequestDto Dto) : IRequest<Response<long>>;
}
