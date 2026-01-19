using BBMB.BudgetService.Shared.RequestDTOs;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Delete
{
    public sealed record DeleteMajorHeadCommand(GetByStringIdRequestDto Request)
        : IRequest<Response>;
}
