using BBMB.BudgetService.Features.DetailHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.DetailHeads.Update
{
    public sealed record UpdateDetailHeadCommand(string DetailAccCode, UpdateDetailHeadDto dto) : IRequest<Response>;
}
