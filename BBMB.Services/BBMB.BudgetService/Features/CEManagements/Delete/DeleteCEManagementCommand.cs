using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.Delete
{
    public sealed record DeleteCEManagementCommand(int ceCode) : IRequest<Response>;
}
