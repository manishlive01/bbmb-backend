using BBMB.BudgetService.Features.Regions.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Update
{
    public sealed record UpdateRegionCommand(string Code, UpdateRegionDto dto) : IRequest<Response>;
}
