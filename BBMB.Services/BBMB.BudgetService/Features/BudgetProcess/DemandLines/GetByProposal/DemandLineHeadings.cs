using BBMB.BudgetService.Features.BudgetProcess.Proposals;
namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.GetByProposal
{
    /// <summary>
    /// BUD-305: every figure heading names its financial year, derived from the cycle in hand, and says
    /// "receipts" for RECEIPTS and "expenditure" otherwise; none reads RBE or OBE alone. A cycle's
    /// <c>current_fin_year</c> is the closing year of <c>fa_financial_years</c> (2026 → 2025-26).
    /// </summary>
    public static class DemandLineHeadings
    {
        public static string YearLabel(int closingYear) => $"{closingYear - 1}-{closingYear % 100:D2}";

        public static Dictionary<string, string> For(string category, int currentFinYear, int nextFinYear)
        {
            var current = YearLabel(currentFinYear);
            var next = YearLabel(nextFinYear);
            var previous = YearLabel(currentFinYear - 1);
            var flow = category == ProposalCategory.Receipts ? "receipts" : "expenditure";
            return new Dictionary<string, string>
            {
                ["openingRbePrevious"] = $"Revised estimate {previous}",
                ["expenditurePreviousYear"] = $"Actual {flow} {previous}",
                ["openingObeCurrent"] = $"Original budget estimate {current}",
                ["expenditureToDate"] = $"Actual {flow} {current} up to date",
                ["rbeAmount"] = $"Revised estimate {current}",
                ["obeNextAmount"] = $"Original budget estimate {next}",
            };
        }
    }
}
