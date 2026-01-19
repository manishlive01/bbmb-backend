using BBMB.BudgetService.Features.ObjectClassifications.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Create
{
    public sealed record CreateObjectClassificationCommand(CreateObjectClassificationDto dto) : IRequest<Response>;
}
