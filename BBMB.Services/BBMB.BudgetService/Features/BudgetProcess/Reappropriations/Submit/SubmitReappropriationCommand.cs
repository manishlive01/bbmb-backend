using BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Submit
{
    public sealed record SubmitReappropriationCommand(long Id, SubmitReappropriationDto Dto) : IRequest<Response>;
}
