using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList.Dtos;
using BBMB.BudgetService.Features.Geographical.States.GetList.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetList
{
    public sealed record GetMajorHeadListCommand() : IRequest<List<GetMajorHeadListResponseDto>>;
}
