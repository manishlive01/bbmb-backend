using BBMB.BudgetService.Shared.ServiceDTOs;

namespace BBMB.BudgetService.Shared.Services.BudgetOffices
{
    /// <summary>A T-13 office as the actor's office.</summary>
    public sealed record ActorOffice(long OfficeId, string OfficeLevel);

    /// <summary>Office resolution and visibility (design §4.4 "Office", BUD-031).</summary>
    public interface IBudgetOfficeScope
    {
        /// <summary>
        /// The actor's T-13 office from the acting role and the charge: FINANCE / SECRETARIAT / IT_CELL by role;
        /// DIVISION, CIRCLE, CE by the charge's division_id, cr_id, ce_id. Null when nothing matches (M-20).
        /// </summary>
        Task<ActorOffice?> ResolveActorOfficeAsync(UserChargeContext charge, string role, CancellationToken ct = default);

        /// <summary>
        /// The office and every descendant through parent_office_id. FINANCE and SECRETARIAT see every office
        /// (SRS §10 "All").
        /// </summary>
        Task<IReadOnlySet<long>> GetVisibleOfficeIdsAsync(long officeId, CancellationToken ct = default);
    }
}
