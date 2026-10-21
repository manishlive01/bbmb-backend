using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetById
{
    public sealed record GetTechnicalSanctionByIdQuery(long TechnicalSanctionId) : IRequest<Response<TechnicalSanctionDto>>;
}
