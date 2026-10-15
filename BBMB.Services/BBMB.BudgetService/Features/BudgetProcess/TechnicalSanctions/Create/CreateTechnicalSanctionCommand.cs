using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Create
{
    /// <summary>Returns the technical sanction id.</summary>
    public sealed record CreateTechnicalSanctionCommand(CreateTechnicalSanctionDto Dto) : IRequest<Response<long>>;
}
