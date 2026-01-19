using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public sealed record DeleteDesignationCommand(
        GetByStringIdRequestDto dto)
        : IRequest<Response>;
}
