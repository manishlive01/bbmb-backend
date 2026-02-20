using BBMB.BudgetService.Features.CRManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Update
{
    public sealed record UpdateCRManagementCommand(
        int Id,
        UpdateCRManagementDto Dto
    ) : IRequest<Response>;
}
