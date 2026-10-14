using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.Budgets.Entities;
using BBMB.BudgetService.Features.Classes.Entities;
using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Estimates.Entities;
using BBMB.BudgetService.Features.FinancialYears.Entities;
using BBMB.BudgetService.Features.Geographical.Districts.Entities;
using BBMB.BudgetService.Features.Geographical.States.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.LevelManagement.Entites;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Features.Offices.OfficeManagements.Entities;
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
    public partial class BudgetContext : DbContext
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
        public DbSet<DivSection> DivSections { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<HeadOfAccount> HeadOfAccounts { get; set; }
        public DbSet<MinorHead> MinorHeads { get; set; }
        public DbSet<SubMinorHead> SubMinorHeads { get; set; }
        public DbSet<ObjectClassification> ObjectClassifications { get; set; }
        public DbSet<AccountOffice> AccountOffices { get; set; }
        public DbSet<RemittanceOffice> RemittanceOffices { get; set; }
        public DbSet<OfficeManagement> OfficeManagements { get; set; }
        public DbSet<Pool> Pools { get; set; }
        public DbSet<PowerIrrigation> PowerIrrigations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<UnitMaster> Units { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<CEManagement> CEManagements { get; set; }
        public DbSet<CRManagement> CRManagements { get; set; }
        public DbSet<CRSection> CRSections { get; set; }
        public DbSet<Levels> Levels { get; set; }
        /// <summary>DDD §1.7: numeric(15,2), whole rupees, |v| &lt;= 9999999999999.</summary>
        public const string RupeesColumnType = "budget.rupees";

        /// <summary>DDD §1.7: numeric(17,2), unrounded figures from Financial Accounting (BUD-266).</summary>
        public const string RupeesPaiseColumnType = "budget.rupees_paise";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT SCHEMA FOR BUDGET
            modelBuilder.HasDefaultSchema("budget");

            base.OnModelCreating(modelBuilder);
            // Money: whole rupees, DDD §1.7 domain budget.rupees (design §4.5). The domain is created by
            // migration BudgetP1RupeesAndMoneyColumns before the columns are altered to it.
            modelBuilder.Entity<BudgetEntry>().Property(e => e.BudgetAmount).HasColumnType(RupeesColumnType);
            modelBuilder.Entity<BudgetEntry>().Property(e => e.RevisedAmount).HasColumnType(RupeesColumnType);
            modelBuilder.Entity<BudgetEntry>().Property(e => e.ProposedBudget).HasColumnType(RupeesColumnType);
            modelBuilder.Entity<Estimate>().Property(e => e.EstimateAmt).HasColumnType(RupeesColumnType);
            modelBuilder.Entity<Work>().Property(e => e.EstimatedCost).HasColumnType(RupeesColumnType);

            // Configure composite key for Estimate
            modelBuilder.Entity<Estimate>()
                .HasKey(e => new { e.WorkCode, e.EstimateCode });

            modelBuilder.Entity<Estimate>()
                .HasOne(e => e.Work)
                .WithMany()
                .HasForeignKey(e => e.WorkCode)
                .OnDelete(DeleteBehavior.Restrict);
             
            // =====================================================
            // SubMajorHead Configuration
            // =====================================================
            modelBuilder.Entity<SubMajorHead>(entity =>
            {
                // 🔹 COMPOSITE PRIMARY KEY
                entity.HasKey(x => new { x.MajorAccCode, x.SubMajorAccCode });

                // 🔹 UNIQUE INDEX (extra safety, optional)
                entity.HasIndex(x => new { x.MajorAccCode, x.SubMajorAccCode })
                      .IsUnique();

                // 🔹 Property rules
                entity.Property(x => x.MajorAccCode)
                      .HasMaxLength(4)
                      .IsRequired();

                entity.Property(x => x.SubMajorAccCode)
                      .HasMaxLength(2)
                      .IsRequired();

                entity.Property(x => x.SubMajorAccDesc)
                      .HasMaxLength(100)
                      .IsRequired();

                // 🔹 FK → MajorHead
                entity.HasOne(x => x.MajorHead)
                      .WithMany() // or .WithMany(m => m.SubMajorHeads) if collection exists
                      .HasForeignKey(x => x.MajorAccCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // =====================================================
            // SubMinorHead Configuration
            // =====================================================
            modelBuilder.Entity<SubMinorHead>(entity =>
            {
                // 🔹 COMPOSITE PRIMARY KEY
                entity.HasKey(x => new { x.MinorAccCode, x.SubMinorAccCode });

                // 🔹 UNIQUE INDEX (optional but recommended)
                entity.HasIndex(x => new { x.MinorAccCode, x.SubMinorAccCode })
                      .IsUnique();

                // 🔹 Column rules
                entity.Property(x => x.MinorAccCode)
                      .HasMaxLength(3)
                      .IsRequired();

                entity.Property(x => x.SubMinorAccCode)
                      .HasMaxLength(2)
                      .IsRequired();

                entity.Property(x => x.SubMinorAccDesc)
                      .HasMaxLength(100)
                      .IsRequired();
            });


            // =====================================================
            // HeadOfAccount Configuration
            // =====================================================
            modelBuilder.Entity<HeadOfAccount>(entity =>
            {
                // 🔹 COMPOSITE PRIMARY KEY
                entity.HasKey(x => new
                {
                    x.MajorAccCode,
                    x.SubMajorAccCode,
                    x.MinorAccCode,
                    x.SubMinorAccCode,
                    x.DetailAccCode,
                    x.ObjectAccCode
                });

                // 🔹 UNIQUE INDEX (Business rule)
                entity.HasIndex(x => new
                {
                    x.MajorAccCode,
                    x.SubMajorAccCode,
                    x.MinorAccCode,
                    x.SubMinorAccCode,
                    x.DetailAccCode,
                    x.ObjectAccCode
                }).IsUnique();

                // 🔹 Property rules
                entity.Property(x => x.MajorAccCode).HasMaxLength(4).IsRequired();
                entity.Property(x => x.SubMajorAccCode).HasMaxLength(2); // DDD T-07 varchar(2) (design §5.2)
                entity.Property(x => x.MinorAccCode).HasMaxLength(3).IsRequired();
                entity.Property(x => x.SubMinorAccCode).HasMaxLength(2);
                entity.Property(x => x.DetailAccCode).HasMaxLength(3).IsRequired();
                entity.Property(x => x.ObjectAccCode).HasMaxLength(3).IsRequired();

                entity.Property(x => x.HeadDesc).HasMaxLength(150).IsRequired();
                entity.Property(x => x.UserCode).HasMaxLength(6).IsRequired();

                // 🔹 FK → MajorHead
                entity.HasOne(x => x.MajorHead)
                      .WithMany()
                      .HasForeignKey(x => x.MajorAccCode)
                      .OnDelete(DeleteBehavior.Restrict);

                // 🔹 FK → MinorHead
                entity.HasOne(x => x.MinorHead)
                      .WithMany()
                      .HasForeignKey(x => new
                      {

                          x.MinorAccCode
                      })
                      .OnDelete(DeleteBehavior.Restrict);

                // 🔹 FK → ObjectClassification
                entity.HasOne(x => x.ObjectClassification)
                      .WithMany()
                      .HasForeignKey(x => x.ObjectAccCode)
                      .OnDelete(DeleteBehavior.Restrict);
                 
            });

            // =============================
            // CE
            // =============================
            modelBuilder.Entity<CEManagement>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ business key
                entity.HasAlternateKey(e => e.CECode);
                entity.HasIndex(e => e.CECode).IsUnique();
            });


            // =============================
            // CR
            // =============================
            modelBuilder.Entity<CRManagement>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ business key
                entity.HasAlternateKey(e => e.CRCode);
                entity.HasIndex(e => e.CRCode).IsUnique();

                // ✅ FK → CE (Id based)
                entity.HasOne(e => e.CEManagement)
                      .WithMany(c => c.CRManagements)
                      .HasForeignKey(e => e.CEId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CRSection>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.CRManagement)
                      .WithMany(c => c.CRSections)
                      .HasForeignKey(e => e.CRId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Division>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ business key
                entity.HasAlternateKey(e => e.DivisionCode);
                entity.HasIndex(e => e.DivisionCode).IsUnique();

                // ✅ FK → CR
                entity.HasOne(d => d.CRManagement)
                      .WithMany(c => c.Divisions)
                      .HasForeignKey(d => d.CRId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Division>()
    .HasOne(d => d.Pool)
    .WithMany(p => p.Divisions)
    .HasForeignKey(d => d.PoolCode)
    .HasPrincipalKey(p => p.PoolCode);
            modelBuilder.Entity<Division>()
    .HasOne(d => d.Region)
    .WithMany(r => r.Divisions)
    .HasForeignKey(d => d.RegionCode)
    .HasPrincipalKey(r => r.RegionCode);
            modelBuilder.Entity<DivSection>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Division)
                      .WithMany(d => d.DivSections)
                      .HasForeignKey(e => e.DivisionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<BudgetEntry>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ Division FK
                entity.HasOne(b => b.Division)
                      .WithMany(d => d.BudgetEntries)
                      .HasForeignKey(b => b.DivisionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ Work FK
                entity.HasOne(b => b.Work)
                      .WithMany(w => w.BudgetEntries)
                      .HasForeignKey(b => b.WorkId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ Optional business rule (avoid duplicate entries)
                entity.HasIndex(e => new { e.DivisionId, e.CalendarYear, e.SequenceNo })
                      .IsUnique();
            });
            modelBuilder.Entity<Work>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ business key
                entity.HasAlternateKey(e => e.WorkCode);
                entity.HasIndex(e => e.WorkCode).IsUnique();

                // ✅ FK → Division
                entity.HasOne(w => w.Division)
                      .WithMany(d => d.Works)
                      .HasForeignKey(w => w.DivisionId)
                      .OnDelete(DeleteBehavior.Restrict);

                // ✅ FK → Designation
                entity.HasOne(w => w.Designation)
                      .WithMany(d => d.Works)
                      .HasForeignKey(w => w.DesignationCode)
                      .HasPrincipalKey(d => d.DesignationCode)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Budget-process tables (design §2.1: partial class BudgetContext.BudgetProcess.cs).
            ConfigureBudgetProcess(modelBuilder);
        }
    }
}
