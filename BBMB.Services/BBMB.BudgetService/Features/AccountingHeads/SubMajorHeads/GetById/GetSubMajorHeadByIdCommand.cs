using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.GetById
{
    public sealed record GetSubMajorHeadByIdCommand(GetByStringIdRequestDto Request)
        : IRequest<GetSubMajorHeadByIdResponseDto?>;
}
