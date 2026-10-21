using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.GetList
{
    public sealed record GetTechnicalSanctionListQuery(int? WorkId) : IRequest<Response<List<TechnicalSanctionDto>>>;
}
