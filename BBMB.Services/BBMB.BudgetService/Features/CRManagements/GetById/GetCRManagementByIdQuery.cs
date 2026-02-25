using BBMB.BudgetService.Features.CRManagements.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.CRManagements.GetById
{
    public sealed record GetCRManagementByIdQuery(string crCode)
        : IRequest<GetCRManagementByIdResponseDto>;
}
