using BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMinorHeads.GetById
{
    public sealed record GetSubMinorHeadByIdCommand(GetByStringIdRequestDto Request)
        : IRequest<GetSubMinorHeadByIdResponseDto?>;
}
