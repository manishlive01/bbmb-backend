using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList.Dtos;
using MediatR;
using System.Collections.Generic;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetList
{
    public sealed record GetMinorHeadListCommand()
        : IRequest<List<GetMinorHeadListResponseDto>>;
}
