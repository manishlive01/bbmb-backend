using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Features.FinancialYears.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.Works.Entities;
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
        public DbSet<BudgetProposal> BudgetProposals { get; set; }
        public DbSet<DemandLine> DemandLines { get; set; }
        public DbSet<FigureVersion> FigureVersions { get; set; }
        public DbSet<Observation> Observations { get; set; }
        public DbSet<ProposalMovement> ProposalMovements { get; set; }
        public DbSet<ExpenditureFigure> ExpenditureFigures { get; set; }
        public DbSet<OpeningFigure> OpeningFigures { get; set; }
        public DbSet<CeRecommendation> CeRecommendations { get; set; }
        public DbSet<RecommendationVersion> RecommendationVersions { get; set; }
        public DbSet<DemandLineDisplayRow> DemandLineDisplay { get; set; }
        public DbSet<DemandByCeRow> DemandByCe { get; set; }

        /// <summary>Partial UK names whose 23505 maps to a catalogue message (design §4.9).</summary>
        public const string UxDemandLineWork = "ux_demand_line_work";
        public const string UxDemandLineHead = "ux_demand_line_head";

        private static void ConfigureBudgetProcess(ModelBuilder modelBuilder)
        {
            // ---------------- T-07 budget_head_master: P1 columns (design §5.2) ----------------
            modelBuilder.Entity<HeadOfAccount>(e =>
            {
                e.Property(x => x.AccountCodeId).UseIdentityAlwaysColumn();
                e.Property(x => x.AccountCodeId).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                e.HasIndex(x => x.AccountCodeId).IsUnique();
                e.Property(x => x.CompleteCode).HasComputedColumnSql(
                    "major_acc_code || '-' || sub_major_acc_code || '-' || minor_acc_code || '-' || " +
                    "sub_minor_acc_code || '-' || detail_acc_code || '-' || object_acc_code", stored: true);
                e.HasIndex(x => x.CompleteCode).IsUnique();
                e.Property(x => x.NotForUse).HasDefaultValue(false);
                ReadOnly(e.Property(x => x.CreatedAt));
                ReadOnly(e.Property(x => x.CreatedBy));
                ReadOnly(e.Property(x => x.CreatedChargeId));
            });

            // ---------------- T-23 work: P1 columns (design §5.2, §5.5 row a) ----------------
            modelBuilder.Entity<Work>(e =>
            {
                e.Property(x => x.PreDdd).HasDefaultValue(false);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.PrecedingAmrWorkId).OnDelete(DeleteBehavior.Restrict);
                ReadOnly(e.Property(x => x.CreatedAt));
                ReadOnly(e.Property(x => x.CreatedBy));
                ReadOnly(e.Property(x => x.CreatedChargeId));
                ReadOnly(e.Property(x => x.UpdatedAt));
                ReadOnly(e.Property(x => x.UpdatedBy));
                ReadOnly(e.Property(x => x.UpdatedChargeId));
            });

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

            // ---------------- T-29 proposal ----------------
            modelBuilder.Entity<BudgetProposal>(e =>
            {
                e.Property(x => x.ProposalId).UseIdentityAlwaysColumn();
                e.HasIndex(x => new { x.CycleId, x.Category, x.OriginOfficeId }).IsUnique();
                e.HasIndex(x => new { x.CycleId, x.Category, x.StateCode });
                e.HasIndex(x => new { x.HolderOfficeId, x.StateCode });
                e.HasOne<BudgetCycle>().WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.OriginOfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.HolderOfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<ProposalState>().WithMany().HasForeignKey(x => x.StateCode).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-30 demand_line ----------------
            modelBuilder.Entity<DemandLine>(e =>
            {
                e.Property(x => x.DemandLineId).UseIdentityAlwaysColumn();
                e.Property(x => x.RbeAmount).HasColumnType(RupeesColumnType);
                e.Property(x => x.ObeNextAmount).HasColumnType(RupeesColumnType);
                e.Property(x => x.PctCheckNotApplied).HasDefaultValue(false);
                e.Property(x => x.FloorCheckUnchecked).HasDefaultValue(false);
                e.Property(x => x.AddedOnReturn).HasDefaultValue(false);
                e.Property(x => x.IsDeleted).HasDefaultValue(false);
                e.HasIndex(x => new { x.ProposalId, x.SerialNo }).IsUnique();
                // A work once in a demand (BUD-104, M-25); one head once in a head-wise demand (VR-14, M-35).
                e.HasIndex(x => new { x.ProposalId, x.WorkId }).IsUnique()
                    .HasDatabaseName(UxDemandLineWork).HasFilter("NOT is_deleted AND line_type = 'WORK'");
                e.HasIndex(x => new { x.ProposalId, x.AccountCodeId }).IsUnique()
                    .HasDatabaseName(UxDemandLineHead).HasFilter("NOT is_deleted AND line_type = 'HEAD'");
                e.HasOne<BudgetProposal>().WithMany().HasForeignKey(x => x.ProposalId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                // account_code_id → budget_head_master(account_code_id) is added by SQL (unique index, not the PK).
                ConfigureActorColumns(e);
            });

            // ---------------- T-31 figure_version (trigger-written only) ----------------
            modelBuilder.Entity<FigureVersion>(e =>
            {
                e.Property(x => x.FigureVersionId).UseIdentityAlwaysColumn();
                e.Property(x => x.Value).HasColumnType(RupeesColumnType);
                e.HasIndex(x => new { x.DemandLineId, x.Figure, x.CreatedAt });
                e.HasOne<DemandLine>().WithMany().HasForeignKey(x => x.DemandLineId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.LevelOfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<ProposalState>().WithMany().HasForeignKey(x => x.StateCode).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-32 observation ----------------
            modelBuilder.Entity<Observation>(e =>
            {
                e.Property(x => x.ObservationId).UseIdentityAlwaysColumn();
                e.Property(x => x.SelectedForReturn).HasDefaultValue(false);
                e.HasIndex(x => x.ProposalId);
                e.HasOne<BudgetProposal>().WithMany().HasForeignKey(x => x.ProposalId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<DemandLine>().WithMany().HasForeignKey(x => x.DemandLineId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.LevelOfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<ProposalMovement>().WithMany().HasForeignKey(x => x.ReturnedByMovementId).OnDelete(DeleteBehavior.Restrict);
                ReadOnly(e.Property(x => x.RepliedAt));
                ReadOnly(e.Property(x => x.RepliedBy));
                ConfigureActorColumns(e);
            });

            // ---------------- T-33 proposal_movement ----------------
            modelBuilder.Entity<ProposalMovement>(e =>
            {
                e.Property(x => x.MovementId).UseIdentityAlwaysColumn();
                e.HasIndex(x => new { x.ProposalId, x.CreatedAt });
                e.HasOne<BudgetProposal>().WithMany().HasForeignKey(x => x.ProposalId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.FromOfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.ToOfficeId).OnDelete(DeleteBehavior.Restrict);
                // The composite FK (from_state, action_code, to_state) → T-16 is added by BudgetP1StateTransitionSeed.
                ConfigureActorColumns(e);
            });

            // ---------------- T-35 expenditure_figure (append-only) ----------------
            modelBuilder.Entity<ExpenditureFigure>(e =>
            {
                e.Property(x => x.ExpenditureFigureId).UseIdentityAlwaysColumn();
                e.Property(x => x.Amount).HasColumnType(RupeesPaiseColumnType);
                e.HasIndex(x => new { x.SubjectType, x.WorkId, x.OfficeId, x.AccountCodeId, x.Nature, x.FinYear, x.Period });
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.OfficeId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<FinancialYear>().WithMany().HasForeignKey(x => x.FinYear).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-36 opening_figure ----------------
            modelBuilder.Entity<OpeningFigure>(e =>
            {
                e.Property(x => x.OpeningFigureId).UseIdentityAlwaysColumn();
                e.Property(x => x.Amount).HasColumnType(RupeesColumnType);
                // UK (cycle, subject, figure_kind); the subject columns are partly null, so nulls are not distinct.
                e.HasIndex(x => new { x.CycleId, x.SubjectType, x.WorkId, x.OfficeId, x.AccountCodeId, x.FigureKind })
                    .IsUnique().AreNullsDistinct(false);
                e.HasOne<BudgetCycle>().WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.OfficeId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-37 ce_recommendation ----------------
            modelBuilder.Entity<CeRecommendation>(e =>
            {
                e.Property(x => x.RecommendationId).UseIdentityAlwaysColumn();
                e.Property(x => x.RbeRecommended).HasColumnType(RupeesColumnType);
                e.Property(x => x.ObeNextRecommended).HasColumnType(RupeesColumnType);
                e.Property(x => x.RedistributionRequired).HasDefaultValue(false);
                e.HasIndex(x => new { x.CycleId, x.Category, x.CeOfficeId, x.AccountCodeId }).IsUnique();
                e.HasOne<BudgetCycle>().WithMany().HasForeignKey(x => x.CycleId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.CeOfficeId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-38 recommendation_version (trigger-written only) ----------------
            modelBuilder.Entity<RecommendationVersion>(e =>
            {
                e.HasKey(x => new { x.RecommendationId, x.VersionNo });
                e.Property(x => x.RbeRecommended).HasColumnType(RupeesColumnType);
                e.Property(x => x.ObeNextRecommended).HasColumnType(RupeesColumnType);
                e.Property(x => x.ConcurrenceRef).HasMaxLength(100);
                e.HasOne<CeRecommendation>().WithMany().HasForeignKey(x => x.RecommendationId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- Views (design §5.4) ----------------
            modelBuilder.Entity<DemandLineDisplayRow>(e =>
            {
                e.HasNoKey().ToView("v_demand_line_display", "budget");
                e.Property(x => x.LevelFigures).HasColumnType("jsonb");
            });
            modelBuilder.Entity<DemandByCeRow>(e => e.HasNoKey().ToView("v_demand_by_ce", "budget"));

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

        }

        /// <summary>A trigger-filled column EF never writes and reads back after save (design §4.3).</summary>
        private static void ReadOnly<TProperty>(Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<TProperty> p)
        {
            p.ValueGeneratedOnAddOrUpdate();
            p.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            p.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
