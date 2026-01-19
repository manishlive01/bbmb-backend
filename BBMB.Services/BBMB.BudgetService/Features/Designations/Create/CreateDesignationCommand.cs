using BBMB.BudgetService.Features.Designations.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Designations.Create
{
    public sealed record CreateDesignationCommand(CreateDesignationDto dto) : IRequest<Response>;
}
