using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;

namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.ReferBack
{
    /// <summary>Design §6.2 T24/T27, §8.6 refer-back. Remark required (M-12).</summary>
    public sealed record ReferBackCommand(long ProposalId, string? Remark) : IRequest<Response>;
}
