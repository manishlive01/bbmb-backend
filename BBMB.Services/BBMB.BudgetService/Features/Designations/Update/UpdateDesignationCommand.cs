using BBMB.BudgetService.Features.Designations.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Update
{
    public record UpdateDesignationCommand(string Code, UpdateDesignationDto dto) : IRequest<Response>;
}
