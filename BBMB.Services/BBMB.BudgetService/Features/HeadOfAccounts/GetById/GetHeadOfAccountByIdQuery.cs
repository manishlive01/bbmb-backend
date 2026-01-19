using BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById
{
    public sealed record GetHeadOfAccountByIdQuery(int Id) : IRequest<GetHeadOfAccountByIdResponseDto>;
}
