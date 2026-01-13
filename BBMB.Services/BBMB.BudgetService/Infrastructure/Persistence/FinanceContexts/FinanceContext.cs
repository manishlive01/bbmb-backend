using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Infrastructure.Persistence.FinanceContexts
{
    public class FinanceContext : DbContext
    {
        public FinanceContext(DbContextOptions<FinanceContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT SCHEMA FOR FINANCE
            modelBuilder.HasDefaultSchema("finance");

            base.OnModelCreating(modelBuilder);

            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceContext).Assembly);
        }
    }
}
