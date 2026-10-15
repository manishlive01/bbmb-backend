using BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.GetStateModel
{
    public sealed record GetStateModelQuery : IRequest<Response<StateModelResponseDto>>;
}
