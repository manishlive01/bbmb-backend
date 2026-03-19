using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites
{
    [Table("levels")]
    public class Levels
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("status")]
        public bool Status { get; set; }
    }
}
