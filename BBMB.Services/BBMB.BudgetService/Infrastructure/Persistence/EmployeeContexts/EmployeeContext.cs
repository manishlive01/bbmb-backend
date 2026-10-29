using BBMB.BudgetService.Features.EmployeeCharges.Entities;
using BBMB.BudgetService.Features.Employees.Entities;
using BBMB.BudgetService.Features.PublicMasters.Entities;
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
        public DbSet<EmploymentDetail> EmploymentDetails { get; set; }
        public DbSet<PhotoDocument> PhotoDocuments { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<EmployeeCharge> EmployeeCharges { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<Title> Titles { get; set; }
        public DbSet<Gender> Genders { get; set; }
        public DbSet<SourceOfRecruitment> SourceOfRecruitments { get; set; }
        public DbSet<AppointmentType> AppointmentTypes { get; set; }
        public DbSet<FamilyRelation> FamilyRelations { get; set; }
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