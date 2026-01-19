using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Delete
{
    public record DeleteDesignationCommand(string Code) : IRequest<Response>;
}
