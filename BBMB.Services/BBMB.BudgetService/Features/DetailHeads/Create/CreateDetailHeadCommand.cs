using BBMB.BudgetService.Features.DetailHeads.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.Create
{
    public sealed record CreateDetailHeadCommand(CreateDetailHeadDto dto) : IRequest<Response>;
}
