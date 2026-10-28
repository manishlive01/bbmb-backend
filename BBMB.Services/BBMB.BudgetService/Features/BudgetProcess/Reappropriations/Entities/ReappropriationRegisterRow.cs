using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Reappropriations.Entities
{
    /// <summary>
    /// V-10 <c>budget.v_reappropriation_register</c> (design §5.4, DDD V-10, F-22; BUD-208, BUD-300): the
    /// re-appropriation register of a year, with the unforeseen authorisations shown as rows of their own and
    /// marked as such (<see cref="EntryKind"/> = REAPPROPRIATION or UNFORESEEN_AUTH). Keyless.
    /// </summary>
    public class ReappropriationRegisterRow
    {
        /// <summary>REAPPROPRIATION (a T-46 entry) or UNFORESEEN_AUTH (a T-50 authorisation).</summary>
        [Column("entry_kind")] public string EntryKind { get; set; } = string.Empty;
        [Column("entry_id")] public long EntryId { get; set; }
        [Column("fin_year")] public int FinYear { get; set; }
        [Column("serial_no")] public int? SerialNo { get; set; }
        [Column("status")] public string? Status { get; set; }
        [Column("amount")] public decimal Amount { get; set; }
        [Column("eoffice_ref")] public string? EofficeRef { get; set; }
        [Column("office_id")] public long OfficeId { get; set; }
        [Column("created_at")] public DateTimeOffset CreatedAt { get; set; }
    }
}
