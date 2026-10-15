using BBMB.BudgetService.Features.BudgetProcess.Observations.Reply.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Reply
{
    public sealed record ReplyObservationCommand(long ProposalId, long ObservationId, ReplyObservationDto Dto) : IRequest<Response>;
}
