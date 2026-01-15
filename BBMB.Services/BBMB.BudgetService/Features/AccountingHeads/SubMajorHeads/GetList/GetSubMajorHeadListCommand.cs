using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList.Dtos;
using MediatR;
using System.Collections.Generic;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetList
{
    public sealed record GetSubMajorHeadListCommand()
        : IRequest<List<GetSubMajorHeadListResponseDto>>;
}
