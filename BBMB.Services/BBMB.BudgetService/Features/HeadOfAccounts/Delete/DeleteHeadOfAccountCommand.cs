using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.HeadOfAccounts.Delete
{
    public sealed record DeleteHeadOfAccountCommand(
     string MajorAccCode,
     string? SubMajorAccCode,
     string MinorAccCode,
     string? SubMinorAccCode,
     string DetailAccCode,
     string ObjectAccCode
 ) : IRequest<Response>;

}
