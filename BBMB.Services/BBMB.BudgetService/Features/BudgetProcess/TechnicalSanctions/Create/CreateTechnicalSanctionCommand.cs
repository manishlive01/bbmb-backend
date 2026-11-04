using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create
{
    /// <summary>Returns the sanction id, its serial, the work and the AM&amp;R demand line added (if any).</summary>
    public sealed record CreateTechnicalSanctionCommand(CreateTechnicalSanctionDto Dto) : IRequest<Response<CreateTechnicalSanctionResponseDto>>;
}
