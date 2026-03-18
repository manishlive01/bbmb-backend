using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Delete
{
    public record DeleteOfficeManagementCommand(string OfficeCode)
        : IRequest<Response>;
}