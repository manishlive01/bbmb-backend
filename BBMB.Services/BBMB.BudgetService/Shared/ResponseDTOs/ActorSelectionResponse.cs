namespace BBMB.BudgetService.Shared.ResponseDTOs
{
    /// <summary>
    /// A charge the client may select when the acting charge is ambiguous (requirement 5, design §5): the
    /// charge id and, when resolvable, the office it maps to and that office's level.
    /// </summary>
    public sealed record SelectableChargeDto(Guid ChargeId, long? OfficeId, string? OfficeLevel);

    /// <summary>
    /// The 409 body for an ambiguous or missing acting-role / acting-charge selection (requirements 3 and 5,
    /// design §4, §5). Carries message id <c>M-20</c> (the existing "not authorised" id — no new catalogue id
    /// is minted, OQ-4); <see cref="Reason"/> and the selectable lists carry the detail so the frontend can
    /// prompt and resend with <c>X-Budget-Role</c> / <c>X-Budget-Charge</c>. Extends <see cref="Response"/> so
    /// <c>IsSucced</c> / <c>Message</c> / <c>MessageId</c> are present for existing clients.
    /// </summary>
    public sealed class ActorSelectionResponse : Response
    {
        /// <summary><c>SEVERAL_ROLES_NO_HEADER</c>, <c>HEADER_ROLE_NOT_HELD</c>, or <c>AMBIGUOUS_CHARGE</c>.</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>The Budget-project roles the user may act under (role-selection reasons only).</summary>
        public IReadOnlyList<string>? SelectableRoles { get; set; }

        /// <summary>The charges the user may act under (charge-selection reason only).</summary>
        public IReadOnlyList<SelectableChargeDto>? SelectableCharges { get; set; }
    }
}
