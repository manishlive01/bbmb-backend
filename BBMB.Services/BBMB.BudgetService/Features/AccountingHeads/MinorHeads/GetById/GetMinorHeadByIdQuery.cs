using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById
{
    public sealed record GetMinorHeadByIdQuery(string MinorAccCode) : IRequest<GetMinorHeadByIdResponseDto>;
}
