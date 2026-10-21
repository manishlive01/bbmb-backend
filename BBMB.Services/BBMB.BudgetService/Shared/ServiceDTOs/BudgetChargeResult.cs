namespace BBMB.BudgetService.Shared.ServiceDTOs
{
    public enum BudgetChargeOutcome
    {
        /// <summary>No current Budget charge (or the header named a charge not held). The caller answers M-20.</summary>
        None,

        /// <summary>Exactly one charge is selected.</summary>
        Single,

        /// <summary>Several current Budget charges and no <c>X-Budget-Charge</c> header: M-20 with the ids, never a pick.</summary>
        Ambiguous,
    }

    /// <summary>Result of <c>IChargeResolver.GetCurrentBudgetChargeAsync</c> (design §4.3 "Charge selection", D-12).</summary>
    public sealed record BudgetChargeResult(
        BudgetChargeOutcome Outcome,
        UserChargeContext? Charge,
        IReadOnlyList<Guid> ChargeIds)
    {
        /// <summary>Header naming the charge to act under when the employee holds several.</summary>
        public const string HeaderName = "X-Budget-Charge";

        public static BudgetChargeResult None { get; } = new(BudgetChargeOutcome.None, null, []);

        public static BudgetChargeResult Single(UserChargeContext charge) =>
            new(BudgetChargeOutcome.Single, charge, [charge.ChargeId]);

        public static BudgetChargeResult Ambiguous(IReadOnlyList<Guid> chargeIds) =>
            new(BudgetChargeOutcome.Ambiguous, null, chargeIds);
    }
}
