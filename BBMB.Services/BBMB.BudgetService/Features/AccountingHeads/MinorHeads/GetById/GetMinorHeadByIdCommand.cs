using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MinorHeads.GetById
{
    public sealed record GetMinorHeadByIdCommand(GetByStringIdRequestDto Request)
        : IRequest<GetMinorHeadByIdResponseDto?>;
}
