using BBMB.BudgetService.Features.CEManagements.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.CEManagements.GetById
{
    public sealed record GetCEManagementByIdQuery(string ceCode)
        : IRequest<Response<GetCEManagementByIdResponseDto>>;
}