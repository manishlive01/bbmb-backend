using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Divisions.Delete
{
    public sealed record DeleteDivisionCommand(int Code) : IRequest<Response>;
}
