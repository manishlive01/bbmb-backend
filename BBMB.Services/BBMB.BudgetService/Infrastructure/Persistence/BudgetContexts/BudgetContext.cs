using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Classes.Entities;
using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.Estimates.Entities;
using BBMB.BudgetService.Features.FinancialYears.Entities;
using BBMB.BudgetService.Features.Geographical.Districts.Entities;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using BBMB.BudgetService.Features.Projects.Entities;
using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Features.Units.Entities;
using BBMB.BudgetService.Features.Works.Entities;
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
        public DbSet<MajorHead> MajorHeads { get; set; }
        public DbSet<SubMajorHead> SubMajorHeads { get; set; }
        public DbSet<BudgetEntry> BudgetEntries { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Designation> Designations { get; set; }
        public DbSet<DetailHead> DetailHeads { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<HeadOfAccount> HeadOfAccounts { get; set; }
        public DbSet<MinorHead> MinorHeads { get; set; }
        public DbSet<SubMinorHead> SubMinorHeads { get; set; }
        public DbSet<ObjectClassification> ObjectClassifications { get; set; }
        public DbSet<AccountOffice> AccountOffices { get; set; }
        public DbSet<RemittanceOffice> RemittanceOffices { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<PowerIrrigation> PowerIrrigations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<UnitMaster> Units { get; set; }
        public DbSet<Work> Works { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT SCHEMA FOR BUDGET
            modelBuilder.HasDefaultSchema("budget");

            base.OnModelCreating(modelBuilder);

            // Configure composite key for Estimate
            modelBuilder.Entity<Estimate>()
                .HasKey(e => new { e.WorkCode, e.EstimateCode });

            modelBuilder.Entity<Estimate>()
                .HasOne(e => e.Work)
                .WithMany()
                .HasForeignKey(e => e.WorkCode)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<SubMajorHead>()
        .HasIndex(x => new { x.MajorAccCode, x.SubMajorAccCode })
        .IsUnique();

            modelBuilder.Entity<SubMinorHead>()
    .HasIndex(x => new { x.MinorAccCode, x.SubMinorAccCode })
    .IsUnique();

            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(BudgetContext).Assembly);
        }
    }
}
