using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.Proposals.FinaliseCategory
{
    /// <summary>
    /// Success / list refusal: <c>Response&lt;FinaliseResponseDto&gt;</c>. A line rule refusal (VR-15):
    /// <c>Response&lt;RuleFailuresDto&gt;</c>. Any other refusal: a plain catalogued <see cref="Response"/>.
    /// </summary>
    public sealed record FinaliseCategoryCommand(long CycleId, string Category) : IRequest<Response>;
}
