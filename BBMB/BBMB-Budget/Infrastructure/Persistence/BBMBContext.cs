using BBMB_Budget.Features.Budgets.Budgets.Entities;
using BBMB_Budget.Features.Budgets.BudgetTypes.Entities;
using BBMB_Budget.Features.Geographical.Districts.Entities;
using BBMB_Budget.Features.Geographical.States.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBMB_Budget.Infrastructure.Persistence
{
    public class BBMBContext : DbContext
    {
        public BBMBContext(DbContextOptions<BBMBContext> options)
            : base(options)
        {
        }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<DistrictMaster> DistrictMasters { get; set; }
        //public DbSet<BudgetMaster> BudgetMasters { get; set; }
        public DbSet<BudgetTypeMaster> BudgetTypeMasters { get; set; }
    }
}
