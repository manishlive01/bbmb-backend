using BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Reports.GetManualOfAccountsCodes
{
    /// <summary>Design §8.8 (<c>GET /api/manual-of-accounts-codes</c>, F-02, BUD-048, BUD-049): V-01 as at the request.</summary>
    public sealed record GetManualOfAccountsCodesQuery : IRequest<Response<ManualOfAccountsResultDto>>;
}
