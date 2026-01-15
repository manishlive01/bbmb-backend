using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Create
{
    public sealed record CreateSubMajorHeadCommand(CreateSubMajorHeadDto dto)
        : IRequest<Response>;
}
