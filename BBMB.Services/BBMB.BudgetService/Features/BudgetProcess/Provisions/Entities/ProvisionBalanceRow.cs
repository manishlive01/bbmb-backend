using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.BudgetProcess.Provisions.Entities
{
    /// <summary>
    /// V-02 <c>budget.v_provision_balance</c> (design §5.4, DDD V-02): each live provision (not superseded) with
    /// the sum of its T-42 movements as the available amount, and whether it is operative for its year and
    /// holder. The operative provision of a year is the RBE provision where one is live and the OBE provision
    /// otherwise. This is the single source of "the budget allotted" and is read by every balance check and by
    /// the DB-23 and DB-26 triggers. Keyless; the balance is computed, never stored.
    /// </summary>
    public class ProvisionBalanceRow
    {
        [Column("provision_id")] public long ProvisionId { get; set; }
        [Column("fin_year")] public int FinYear { get; set; }
        [Column("basis")] public string Basis { get; set; } = string.Empty;
        [Column("category")] public string Category { get; set; } = string.Empty;
        [Column("holder_type")] public string HolderType { get; set; } = string.Empty;
        [Column("work_id")] public int? WorkId { get; set; }
        [Column("office_id")] public long OfficeId { get; set; }
        [Column("account_code_id")] public long? AccountCodeId { get; set; }
        [Column("demand_line_id")] public long? DemandLineId { get; set; }
        [Column("effective_from")] public DateOnly EffectiveFrom { get; set; }
        [Column("budgetary_check")] public bool BudgetaryCheck { get; set; }
        /// <summary>The sum of the provision's T-42 movements.</summary>
        [Column("available")] public decimal Available { get; set; }
        /// <summary>True for the RBE provision where one is live, the OBE provision otherwise.</summary>
        [Column("is_operative")] public bool IsOperative { get; set; }
    }
}
