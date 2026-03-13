using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Features.Units.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Divisions.Entities
{
    [Table("division")]
    public class Division
    {
        [Key]
        [Required]
        [Column("division_code")]
        public int DivisionCode { get; set; }

        [Required]
        [Column("div_section")]
        public int DivSection { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("division_desc")]
        public string DivisionDesc { get; set; } = string.Empty;

        [Column("ctrl_office1")]
        public int? CtrlOffice1 { get; set; }

        [Column("ctrl_office2")]
        public int? CtrlOffice2 { get; set; }

        [MaxLength(3)]
        [Column("ro_code")]
        public string? RoCode { get; set; }

        [MaxLength(4)]
        [Column("major_acc_code")]
        public string? MajorAccCode { get; set; }

        [MaxLength(2)]
        [Column("acc_office")]
        public string? AccOffice { get; set; }

        [MaxLength(2)]
        [Column("unit_code")]
        public string? UnitCode { get; set; }

        [MaxLength(1)]
        [Column("pool_code")]
        public string? PoolCode { get; set; }

        [MaxLength(3)]
        [Column("umd_unit")]
        public string? UmdUnit { get; set; }

        [MaxLength(1)]
        [Column("region_code")]
        public string? RegionCode { get; set; }

        [MaxLength(15)]
        [Column("ddo_tds")]
        public string? DdoTds { get; set; }

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Column("parent_division_code")]
        public int? ParentDivisionCode { get; set; }


        [Required]
        [Column("ce_code")]
        public int CECode { get; set; }

        [Required]
        [Column("cr_code")]
        public int CRCode { get; set; }

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;

        // Navigation properties for foreign keys
        public virtual RemittanceOffice? RemittanceOffice { get; set; }
        public virtual MajorHead? MajorHead { get; set; }
        public virtual AccountOffice? AccountOffice { get; set; }
        public virtual UnitMaster? UnitMasters { get; set; }
        public virtual Pool? Pool { get; set; }
        public virtual Region? Region { get; set; }

        [ForeignKey(nameof(CECode))]
        public virtual CEManagement? CEManagement { get; set; }

        [ForeignKey(nameof(CRCode))]
        public virtual CRManagement? CRManagement { get; set; }
    }
}
