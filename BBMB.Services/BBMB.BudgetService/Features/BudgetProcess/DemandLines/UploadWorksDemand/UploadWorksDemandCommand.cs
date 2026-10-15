using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.UploadWorksDemand
{
    /// <summary>
    /// Design §8.5 (<c>POST /api/budget-proposals/{id}/upload</c>, F-08, BUD-098 to BUD-104). The success body is
    /// <c>Response&lt;UploadResultDto&gt;</c>; a refusal is <c>Response</c> (M-11, M-20, M-19, M-17, M-23, or the
    /// line-count limit) or <c>Response&lt;RuleFailuresDto&gt;</c> with the failing rows (BUD-100).
    /// </summary>
    public sealed record UploadWorksDemandCommand(long ProposalId, IFormFile File) : IRequest<Response>;
}
