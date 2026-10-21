using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities
{
    /// <summary>
    /// T-31 <c>budget.figure_version</c>: every value a figure has held, append-only (DDD T-31, DB-14).
    /// Written only by trigger <c>budget.fn_capture_figure_version</c>; the application reads it and never
    /// adds a row (design §5.3).
    /// </summary>
    [Table("figure_version")]
    public class FigureVersion : CreationStampedEntity
    {
        [Key]
        [Column("figure_version_id")]
        public long FigureVersionId { get; private set; }

        [Column("demand_line_id")]
        public long DemandLineId { get; private set; }

        /// <summary>RBE or OBE_NEXT.</summary>
        [Column("figure")]
        [MaxLength(8)]
        public string Figure { get; private set; } = string.Empty;

        [Column("value")]
        public decimal? Value { get; private set; }

        /// <summary>The holder of the proposal at the time (BUD-115, BUD-133).</summary>
        [Column("level_office_id")]
        public long LevelOfficeId { get; private set; }

        [Column("state_code")]
        [MaxLength(4)]
        public string StateCode { get; private set; } = string.Empty;

        /// <summary>ENTRY, UPLOAD or ALTERATION.</summary>
        [Column("change_source")]
        [MaxLength(10)]
        public string ChangeSource { get; private set; } = string.Empty;

        [Column("justification")]
        [MaxLength(4000)]
        public string? Justification { get; private set; }

        [Column("continued_without_justification")]
        public bool ContinuedWithoutJustification { get; private set; }

        [Column("role_code")]
        [MaxLength(50)]
        public string RoleCode { get; private set; } = string.Empty;

        [Column("on_behalf_of_post")]
        [MaxLength(30)]
        public string? OnBehalfOfPost { get; private set; }
    }
}
