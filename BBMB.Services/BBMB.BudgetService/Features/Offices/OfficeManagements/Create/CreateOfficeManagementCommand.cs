using BBMB.BudgetService.Features.Offices.OfficeManagements.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.OfficeManagements.Create
{
    public record CreateOfficeManagementCommand(CreateOfficeManagementDto dto)
        : IRequest<Response>;
}