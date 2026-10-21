using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyOpeningFigure
{
    /// <summary>Success: <c>Response&lt;long&gt;</c> with the T-36 row id.</summary>
    public sealed record KeyOpeningFigureCommand(long ProposalId, long DemandLineId, KeyOpeningFigureDto Dto) : IRequest<Response>;

    /// <summary>T-36 <c>figure_kind</c> values (CHECK).</summary>
    public static class OpeningFigureKind
    {
        public const string ObeCurrent = "OBE_CURRENT";
        public const string RbePrevious = "RBE_PREVIOUS";
        public static readonly string[] All = [ObeCurrent, RbePrevious];
    }
}
