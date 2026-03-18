using BBMB.BudgetService.Features.Offices.OfficeManagements.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Offices.OfficeManagements.Update
{
    public record UpdateOfficeManagementCommand(UpdateOfficeManagementDto dto)
        : IRequest<Response>;
}