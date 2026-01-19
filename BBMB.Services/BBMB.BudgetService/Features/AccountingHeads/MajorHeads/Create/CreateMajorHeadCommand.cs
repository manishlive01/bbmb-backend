using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Create
{
    public sealed record CreateMajorHeadCommand(CreateMajorHeadDto dto) : IRequest<Response>;
}
