using BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure.Dtos;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.KeyExpenditure
{
    /// <summary>Success: <c>Response&lt;long&gt;</c> with the T-35 row id.</summary>
    public sealed record KeyExpenditureCommand(long ProposalId, long DemandLineId, KeyExpenditureDto Dto) : IRequest<Response>;

    /// <summary>T-35 <c>nature</c>, <c>period</c> and <c>source</c> values (CHECKs).</summary>
    public static class ExpenditureValues
    {
        public const string Expenditure = "EXPENDITURE";
        public const string Receipt = "RECEIPT";
        public const string FullYear = "FULL_YEAR";
        public const string UpToMonth = "UP_TO_MONTH";
        public const string UpToDate = "UP_TO_DATE";
        public const string Keyed = "KEYED";
        public static readonly string[] Natures = [Expenditure, Receipt];
        public static readonly string[] Periods = [FullYear, UpToMonth, UpToDate];
    }
}
