using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BBMB.BudgetService.Shared.Entities;

namespace BBMB.BudgetService.Features.BudgetProcess.Offices.Entities
{
    /// <summary>
    /// T-13 <c>budget.office</c>: every office that frames, examines, scrutinises or receives a budget
    /// (DDD T-13, design §5.3). A Division, Circle or CE row refers to its master and holds nothing the master
    /// holds; the Secretariat, Finance and IT Cell rows carry their name. <see cref="ParentOfficeId"/> is the
    /// review channel.
    /// </summary>
    [Table("office")]
    public class BudgetOffice : ActorStampedEntity
    {
        [Key]
        [Column("office_id")]
        public long OfficeId { get; set; }

        [Column("office_level")]
        [MaxLength(12)]
        public string OfficeLevel { get; set; } = string.Empty;

        [Column("division_id")]
        public int? DivisionId { get; set; }

        [Column("circle_id")]
        public int? CircleId { get; set; }

        [Column("ce_id")]
        public int? CeId { get; set; }

        [Column("office_name")]
        [MaxLength(200)]
        public string? OfficeName { get; set; }

        [Column("wing_code")]
        [MaxLength(2)]
        public string? WingCode { get; set; }

        [Column("parent_office_id")]
        public long? ParentOfficeId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        /// <summary>xmin (DDD §1.7, design §4.10).</summary>
        [Timestamp]
        public uint Version { get; set; }
    }

    /// <summary>T-13 <c>office_level</c> values (CHECK).</summary>
    public static class OfficeLevel
    {
        public const string Division = "DIVISION";
        public const string Circle = "CIRCLE";
        public const string Ce = "CE";
        public const string Secretariat = "SECRETARIAT";
        public const string Finance = "FINANCE";
        public const string ItCell = "IT_CELL";

        public static readonly IReadOnlyList<string> All = [Division, Circle, Ce, Secretariat, Finance, ItCell];
    }
}
