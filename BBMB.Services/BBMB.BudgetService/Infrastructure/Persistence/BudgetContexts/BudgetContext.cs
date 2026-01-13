using BBMB.BudgetService.Features.Budgets.BudgetTypes.Entities;
using BBMB.BudgetService.Features.Geographical.Districts.Entities;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts
{
    public class BudgetContext : DbContext
    {
        public BudgetContext(DbContextOptions<BudgetContext> options)
            : base(options)
        {
        }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<DistrictMaster> DistrictMasters { get; set; }
        //public DbSet<BudgetMaster> BudgetMasters { get; set; }
        public DbSet<BudgetTypeMaster> BudgetTypeMasters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT SCHEMA FOR BUDGET
            modelBuilder.HasDefaultSchema("budget");

            base.OnModelCreating(modelBuilder);

            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetContext).Assembly);
        }
    }
}
