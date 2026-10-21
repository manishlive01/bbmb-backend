using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Revise
{
    /// <summary>Returns the new version number; the message states when VR-11 was not applied (OI-15).</summary>
    public sealed record ReviseTechnicalSanctionCommand(long TechnicalSanctionId, ReviseTechnicalSanctionDto Dto) : IRequest<Response<int>>;
}
