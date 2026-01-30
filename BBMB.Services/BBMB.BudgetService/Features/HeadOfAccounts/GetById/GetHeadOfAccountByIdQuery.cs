using BBMB.BudgetService.Features.HeadOfAccounts.GetById.Dtos;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.GetById
{
    public sealed record GetHeadOfAccountByIdQuery(
    string MajorAccCode,
    string? SubMajorAccCode,
    string MinorAccCode,
    string? SubMinorAccCode,
    string DetailAccCode,
    string ObjectAccCode
) : IRequest<GetHeadOfAccountByIdResponseDto>;

}
