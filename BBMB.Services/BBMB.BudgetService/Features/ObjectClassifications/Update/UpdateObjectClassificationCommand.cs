using BBMB.BudgetService.Features.ObjectClassifications.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Update
{
    public record UpdateObjectClassificationCommand(string Code, UpdateObjectClassificationDto dto) : IRequest<Response>;
}
