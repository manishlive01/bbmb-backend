namespace BBMB.BudgetService.Features.HeadOfAccounts.SetPartyEmpFlag.Dtos
{
    /// <summary>Body of <c>PUT /api/head-of-accounts/{id}/party-emp-flag</c> (F-01 column 16, ACC-009, BUD-268).</summary>
    public sealed class SetPartyEmpFlagDto
    {
        /// <summary>Y or N.</summary>
        public string Value { get; set; } = string.Empty;
    }
}
