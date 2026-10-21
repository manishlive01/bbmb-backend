using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Create
{
    public sealed record CreateReappropriationCommand(CreateReappropriationDto Dto) : IRequest<Response<long>>;
}
