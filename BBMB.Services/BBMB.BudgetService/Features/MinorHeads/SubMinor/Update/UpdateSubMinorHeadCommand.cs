using BBMB.BudgetService.Features.MinorHeads.SubMinor.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.MinorHeads.SubMinor.Update
{
    public record UpdateSubMinorHeadCommand(string SubMinorAccCode, UpdateSubMinorHeadDto dto) : IRequest<Response>;
}
