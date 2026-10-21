using BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.UnforeseenWorks.Authorise
{
    public sealed record AuthoriseUnforeseenCommand(long Id, AuthoriseUnforeseenDto Dto) : IRequest<Response>;
}
