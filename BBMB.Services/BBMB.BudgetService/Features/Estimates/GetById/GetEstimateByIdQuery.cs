using BBMB.BudgetService.Features.Estimates.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetById
{
    public record GetEstimateByIdQuery(int WorkCode, string EstimateCode) : IRequest<Response<GetEstimateByIdResponseDto>>;
}
