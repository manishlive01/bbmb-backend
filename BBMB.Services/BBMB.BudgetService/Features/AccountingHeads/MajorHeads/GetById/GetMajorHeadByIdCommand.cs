using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById.Dtos;
using BBMB.BudgetService.Features.Geographical.States.GetById.Dtos;
using BBMB.BudgetService.Shared.RequestDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.GetById
{
    public sealed record GetMajorHeadByIdCommand(GetByStringIdRequestDto Request) : IRequest<GetMajorHeadByIdResponseDto?>;
}
