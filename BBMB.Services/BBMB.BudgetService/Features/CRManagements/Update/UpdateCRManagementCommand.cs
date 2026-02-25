using BBMB.BudgetService.Features.CRManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Update
{
    public sealed record UpdateCRManagementCommand(
        string crCode,
        UpdateCRManagementDto Dto
    ) : IRequest<Response>;
}
