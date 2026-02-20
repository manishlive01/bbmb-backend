using BBMB.BudgetService.Features.CRManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Create
{
    public sealed record CreateCRManagementCommand(
        CreateCRManagementDto Dto
    ) : IRequest<Response>;
}
