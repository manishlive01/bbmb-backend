using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Regions.Delete
{
    public sealed record DeleteRegionCommand(string Code) : IRequest<Response>;
}
