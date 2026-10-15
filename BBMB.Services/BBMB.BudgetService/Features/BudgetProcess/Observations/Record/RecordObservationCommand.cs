using BBMB.BudgetService.Features.BudgetProcess.Observations.Record.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Observations.Record
{
    public sealed record RecordObservationCommand(long ProposalId, RecordObservationDto Dto) : IRequest<Response<long>>;
}
