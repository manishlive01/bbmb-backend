using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.Pools.Entities
{
    [Table("pool")]
    public class Pool
    {
        [Key]
        [MaxLength(1)]
        [Column("pool_code")]
        public string PoolCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [Column("pool_desc")]
        public string PoolDesc { get; set; } = string.Empty;

        [Required]
        [Column("modification_date")]
        public DateTime ModificationDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(6)]
        [Column("user_code")]
        public string UserCode { get; set; } = string.Empty;
    }
}
