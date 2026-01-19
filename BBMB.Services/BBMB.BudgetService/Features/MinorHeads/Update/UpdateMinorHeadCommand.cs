using BBMB.BudgetService.Features.MinorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.Update
{
    public sealed record UpdateMinorHeadCommand(string MinorAccCode, UpdateMinorHeadDto dto) : IRequest<Response>;
}
