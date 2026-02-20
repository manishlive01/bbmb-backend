using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.Delete
{
    public sealed record DeleteCRManagementCommand(int Id)
        : IRequest<Response>;
}
