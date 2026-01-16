using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList.Dtos;
using MediatR;
using System.Collections.Generic;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetList
{
    public sealed record GetSubMinorHeadListCommand()
        : IRequest<List<GetSubMinorHeadListResponseDto>>;
}
