using BBMB.BudgetService.Features.Works.GenerateWorkCodes.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Works.GenerateWorkCodes
{
    public record GenerateWorkCodeQuery(int DivisionCode)
        : IRequest<Response<GenerateWorkCodeResponseDto>>;
}