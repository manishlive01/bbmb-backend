using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Update
{
    public sealed record UpdateMajorHeadCommand(
        UpdateMajorHeadDto dto
    ) : IRequest<Response>;
}
