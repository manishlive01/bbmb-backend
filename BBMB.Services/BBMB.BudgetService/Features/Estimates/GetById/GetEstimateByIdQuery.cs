using BBMB.BudgetService.Features.Estimates.GetById.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.Estimates.GetById
{
    // P4 (design §4.6): the estimate is keyed on (work_id, estimate_code); the GET is addressed by work id.
    public record GetEstimateByIdQuery(int WorkId, string EstimateCode) : IRequest<Response<GetEstimateByIdResponseDto>>;
}
