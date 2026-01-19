using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public sealed record GetSubMajorHeadByIdQuery(string SubMajorAccCode) : IRequest<GetSubMajorHeadByIdResponseDto>;
}
