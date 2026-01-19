using BBMB.BudgetService.Features.Units.GetByProject.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Units.GetByProject
{
    public sealed record GetUnitsByProjectQuery(string ProjCode) : IRequest<Response<List<GetUnitsByProjectResponseDto>>>;
}
