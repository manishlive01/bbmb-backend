using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.Delete
{
    public record DeleteObjectClassificationCommand(string Code) : IRequest<Response>;
}
