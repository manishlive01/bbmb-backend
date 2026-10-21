using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.GetList
{
    /// <summary><paramref name="OfficeId"/> null means the actor's own office.</summary>
    public sealed record GetHeadAssignmentsQuery(long? OfficeId) : IRequest<Response<List<AssignedHeadDto>>>;
}
