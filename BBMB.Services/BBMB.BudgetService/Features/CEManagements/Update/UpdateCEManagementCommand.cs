using BBMB.BudgetService.Features.CEManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Update
{
    public sealed record UpdateCEManagementCommand(
        string ceCode,
        UpdateCEManagementDto Dto
    ) : IRequest<Response>;
}
