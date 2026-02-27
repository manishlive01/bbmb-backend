using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Works.Entities
{
    [Table("work")]
    public class Work
    {
        [Key]
        [Required]
        [MaxLength(8)]
        [Column("work_code")]
        public string WorkCode { get; set; }

        [Required]
        [Column("division_code")]
        public int DivisionCode { get; set; }

        [Required]
        [Column("div_section")]
        public int DivSection { get; set; }

        [Required]
        [MaxLength(4)]
        [Column("major_acc_code")]
        public string MajorAccCode { get; set; }

        [Required]
        [MaxLength(2)]
        [Column("sub_major_acc_code")]
        public string SubMajorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("minor_acc_code")]
        public string MinorAccCode { get; set; }

        [Required]
        [MaxLength(2)]
        [Column("sub_minor_acc_code")]
        public string SubMinorAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("detail_acc_code")]
        public string DetailAccCode { get; set; }

        [Required]
        [MaxLength(3)]
        [Column("object_acc_code")]
        public string ObjectAccCode { get; set; }

        [MaxLength(4)]
        [Column("fin_yr_start")]
        public string? FinYrStart { get; set; }

        [MaxLength(4)]
        [Column("fin_yr_end")]
        public string? FinYrEnd { get; set; }

        [MaxLength(20)]
        [Column("sanction_no")]
        public string? SanctionNo { get; set; }

        [MaxLength(11)]
        [Column("sanction_date")]
        public string? SanctionDate { get; set; }

        [MaxLength(6)]
        [Column("designation_code")]
        public string? DesignationCode { get; set; }

        [Column("estimated_cost")]
        [Precision(12, 2)]
        public decimal? EstimatedCost { get; set; }

        [MaxLength(1)]
        [Column("status")]
        public string? Status { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("work_desc")]
        public string WorkDesc { get; set; } = string.Empty;

        [MaxLength(1)]
        [Column("nature")]
        public string? Nature { get; set; }

        [MaxLength(8)]
        [Column("ref_work_code")]
        public string? RefWorkCode { get; set; }

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties for foreign keys
        public virtual Division? Division { get; set; }
        public virtual MajorHead? MajorHead { get; set; }
        public virtual SubMajorHead? SubMajorHead { get; set; }
        public virtual MinorHead? MinorHead { get; set; }
        public virtual SubMinorHead? SubMinorHead { get; set; }
        public virtual DetailHead? DetailHead { get; set; }
        public virtual ObjectClassification? ObjectClassification { get; set; }
        public virtual Designation? Designation { get; set; }

    }
}
