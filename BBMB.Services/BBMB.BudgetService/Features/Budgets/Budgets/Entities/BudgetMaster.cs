using BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Budgets.Budgets.Entities
{
    [Table("budget_master")]
    public class BudgetMaster
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("division_code")]
        public int DivisionCode { get; set; }

        [Required]
        [Column("div_section")]
        public int DivSection { get; set; }

        [Required]
        [Column("calendar_year")]
        public int CalendarYear { get; set; }

        [Required]
        [Column("sequence_no")]
        public int SequenceNo { get; set; }

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string SubMajorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(3)]
        [Column("object_acc_code")]
        public string ObjectAccCode { get; set; } = string.Empty;

        [MaxLength(8)]
        [Column("work_code")]
        public string? WorkCode { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("budget_description")]
        public string BudgetDescription { get; set; } = string.Empty;

        [Column("budget_amount")]
        [Precision(12, 2)]
        public decimal? BudgetAmount { get; set; }

        [Column("revised_amount")]
        [Precision(12, 2)]
        public decimal? RevisedAmount { get; set; }

        [Column("proposed_budget")]
        [Precision(12, 2)]
        public decimal? ProposedBudget { get; set; }

        public int? BudgetTypeMasterId { get; set; }
        [ForeignKey(nameof(BudgetTypeMasterId))]
        public virtual BudgetTypeMaster BudgetTypeMasters { get; set; }

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties for foreign keys
        //public virtual Division? Division { get; set; }
        //public virtual MajorHead? MajorHead { get; set; }
        //public virtual SubMajorHead? SubMajorHead { get; set; }
        //public virtual MinorHead? MinorHead { get; set; }
        //public virtual SubMinorHead? SubMinorHead { get; set; }
        //public virtual DetailHead? DetailHead { get; set; }
        //public virtual ObjectClassification? ObjectClassification { get; set; }
        //public virtual Work? Work { get; set; }
    }
}
