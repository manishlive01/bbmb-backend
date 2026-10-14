using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Features.FinancialYears.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Features.PowerIrrigations.Entities;
using BBMB.BudgetService.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts
{
    /// <summary>
    /// Budget-process tables (design §2.1, §5). Keys, indexes and FKs are configured here; CHECKs, triggers
    /// and the cross-schema FKs to employee.* are hand SQL in the migrations (design §11.1 rule 2).
    /// </summary>
    public partial class BudgetContext
    {
        public DbSet<BudgetOffice> BudgetOffices { get; set; }
        public DbSet<AuditLogEntry> AuditLog { get; set; }
        public DbSet<BudgetCycle> BudgetCycles { get; set; }
        public DbSet<ProposalState> ProposalStates { get; set; }
        public DbSet<BudgetSetting> BudgetSettings { get; set; }

        private static void ConfigureBudgetProcess(ModelBuilder modelBuilder)
        {
            // ---------------- T-13 office ----------------
            modelBuilder.Entity<BudgetOffice>(e =>
            {
                e.Property(x => x.OfficeId).UseIdentityAlwaysColumn();
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.HasIndex(x => x.DivisionId).IsUnique();
                e.HasIndex(x => x.CircleId).IsUnique();
                e.HasIndex(x => x.CeId).IsUnique();
                e.HasIndex(x => x.ParentOfficeId);
                e.HasOne<Division>().WithMany().HasForeignKey(x => x.DivisionId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<CRManagement>().WithMany().HasForeignKey(x => x.CircleId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<CEManagement>().WithMany().HasForeignKey(x => x.CeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<PowerIrrigation>().WithMany().HasForeignKey(x => x.WingCode).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.ParentOfficeId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-51 audit_log ----------------
            modelBuilder.Entity<AuditLogEntry>(e =>
            {
                e.Property(x => x.AuditId).UseIdentityAlwaysColumn();
                e.Property(x => x.OccurredAt).HasDefaultValueSql("clock_timestamp()");
                e.Property(x => x.TransactionId).HasDefaultValueSql("txid_current()");
                e.Property(x => x.Operation).IsFixedLength();
                e.HasIndex(x => x.OccurredAt);
                e.HasIndex(x => new { x.TableName, x.RecordKey });
                e.HasIndex(x => new { x.ActorOfficeId, x.OccurredAt });
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.ActorOfficeId).OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- T-14 budget_cycle ----------------
            modelBuilder.Entity<BudgetCycle>(e =>
            {
                e.Property(x => x.CycleId).UseIdentityAlwaysColumn();
                e.HasIndex(x => x.CurrentFinYear).IsUnique();
                e.HasIndex(x => x.IsFirstCycle).IsUnique().HasFilter("is_first_cycle");
                e.HasOne<FinancialYear>().WithMany().HasForeignKey(x => x.CurrentFinYear).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<FinancialYear>().WithMany().HasForeignKey(x => x.NextFinYear).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-15 state ----------------
            modelBuilder.Entity<ProposalState>(e =>
            {
                e.HasKey(x => x.StateCode);
            });

            // ---------------- T-17 setting ----------------
            modelBuilder.Entity<BudgetSetting>(e =>
            {
                e.Property(x => x.SettingId).UseIdentityAlwaysColumn();
                e.HasIndex(x => new { x.SettingKey, x.EffectiveFrom }).IsUnique();
                ConfigureActorColumns(e);
            });
        }

        /// <summary>
        /// Maps the [A]/[C] actor columns (DDD §1.7) as database-generated and never written by EF: they are
        /// filled by <c>budget.fn_require_actor</c> and read back through RETURNING (design §4.3).
        /// </summary>
        private static void ConfigureActorColumns<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> e)
            where T : CreationStampedEntity
        {
            ReadOnly(e.Property(x => x.CreatedAt));
            ReadOnly(e.Property(x => x.CreatedBy));
            ReadOnly(e.Property(x => x.CreatedChargeId));

            if (typeof(ActorStampedEntity).IsAssignableFrom(typeof(T)))
            {
                ReadOnly(e.Property<DateTimeOffset?>(nameof(ActorStampedEntity.UpdatedAt)));
                ReadOnly(e.Property<Guid?>(nameof(ActorStampedEntity.UpdatedBy)));
                ReadOnly(e.Property<Guid?>(nameof(ActorStampedEntity.UpdatedChargeId)));
            }

            static void ReadOnly<TProperty>(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<TProperty> p)
            {
                p.ValueGeneratedOnAddOrUpdate();
                p.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                p.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            }
        }
    }
}
