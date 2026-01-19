using BBMB.BudgetService.Features.ObjectClassifications.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.ObjectClassifications.GetById
{
    public sealed record GetObjectClassificationByIdQuery(string Code) : IRequest<GetObjectClassificationByIdResponseDto>;
}
