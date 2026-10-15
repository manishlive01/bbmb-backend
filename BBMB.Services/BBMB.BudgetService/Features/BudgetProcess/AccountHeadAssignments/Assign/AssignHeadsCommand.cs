using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Assign
{
    /// <summary>Returns the number of heads newly assigned.</summary>
    public sealed record AssignHeadsCommand(AssignHeadsDto Dto) : IRequest<Response<int>>;
}
