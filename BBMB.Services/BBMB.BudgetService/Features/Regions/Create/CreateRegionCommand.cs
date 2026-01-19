using BBMB.BudgetService.Features.Regions.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Create
{
    public sealed record CreateRegionCommand(CreateRegionDto dto) : IRequest<Response>;
}
