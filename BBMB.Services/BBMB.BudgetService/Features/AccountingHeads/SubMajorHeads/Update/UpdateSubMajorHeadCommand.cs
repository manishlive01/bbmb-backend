using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Update
{
    public sealed record UpdateSubMajorHeadCommand(string SubMajorAccCode, UpdateSubMajorHeadDto dto) : IRequest<Response>;
}
