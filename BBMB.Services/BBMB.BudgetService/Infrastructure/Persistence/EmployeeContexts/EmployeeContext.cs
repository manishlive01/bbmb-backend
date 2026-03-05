using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using Microsoft.EntityFrameworkCore;

namespace BBMB.BudgetService.Infrastructure.Persistence.EmployeeContexts
{
    /// <summary>
    /// EmployeeContext
    /// ----------------
    /// EF Core DbContext for Employee module.
    ///
    /// Purpose:
    /// - Manages Employee database entities
    /// - Uses separate schema for clean database separation
    /// - Supports migrations specific to Employee module
    ///
    /// Lifetime:
    /// - Registered as Scoped (1 instance per HTTP request)
    /// </summary>
    public class EmployeeContext : DbContext
    {
        public EmployeeContext(DbContextOptions<EmployeeContext> options)
            : base(options)
        {

        }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeCharge> EmployeeCharges { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT SCHEMA FOR EMPLOYEE
            modelBuilder.HasDefaultSchema("employee");

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EmployeeCharge>()
    .HasOne(x => x.Employee)
    .WithMany(x => x.Charges)
    .HasForeignKey(x => x.EmployeeId);
            // Apply all IEntityTypeConfiguration from this assembly
            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeContext).Assembly);
        }
    }
}