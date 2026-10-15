using BBMB.BudgetService.Features.BudgetProcess.AccountHeadAssignments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.AdminApprovals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Attachments.Entities;
using BBMB.BudgetService.Features.BudgetProcess.AuditTrail.Entities;
using BBMB.BudgetService.Features.BudgetProcess.CorrectionFlags.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Cycles.Entities;
using BBMB.BudgetService.Features.BudgetProcess.DemandLines.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Observations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Offices.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Proposals.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Recommendations.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Reports.Entities;
using BBMB.BudgetService.Features.BudgetProcess.Settings.Entities;
using BBMB.BudgetService.Features.BudgetProcess.TechnicalSanctions.Entities;
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
        public DbSet<StateTransition> StateTransitions { get; set; }
        public DbSet<BudgetSetting> BudgetSettings { get; set; }
        public DbSet<BudgetProposal> BudgetProposals { get; set; }
        public DbSet<DemandLine> DemandLines { get; set; }
        public DbSet<UploadBatch> UploadBatches { get; set; }
        public DbSet<FigureVersion> FigureVersions { get; set; }
        public DbSet<Observation> Observations { get; set; }
        public DbSet<ProposalMovement> ProposalMovements { get; set; }
        public DbSet<ExpenditureFigure> ExpenditureFigures { get; set; }
        public DbSet<OpeningFigure> OpeningFigures { get; set; }
        public DbSet<CeRecommendation> CeRecommendations { get; set; }
        public DbSet<RecommendationVersion> RecommendationVersions { get; set; }
        public DbSet<DemandLineDisplayRow> DemandLineDisplay { get; set; }
        public DbSet<DemandByCeRow> DemandByCe { get; set; }
        // P2 (design §5.3 P2 table)
        public DbSet<BudgetAttachment> BudgetAttachments { get; set; }
        public DbSet<DelegationLimit> DelegationLimits { get; set; }
        public DbSet<AdminApproval> AdminApprovals { get; set; }
        public DbSet<AdminApprovalVersion> AdminApprovalVersions { get; set; }
        public DbSet<TechnicalSanction> TechnicalSanctions { get; set; }
        public DbSet<TechnicalSanctionVersion> TechnicalSanctionVersions { get; set; }
        public DbSet<CorrectionFlag> CorrectionFlags { get; set; }
        public DbSet<AccountHeadAssignment> AccountHeadAssignments { get; set; }
        public DbSet<ManualOfAccountsCodeRow> ManualOfAccountsCodes { get; set; }

        /// <summary>Partial UK names whose 23505 maps to a catalogue message (design §4.9).</summary>
        /// <summary>T-21 partial UK (office, account code) where removed_at is null.</summary>
        public const string UxAccountHeadAssignmentLive = "ux_account_head_assignment_live";
        public const string UxDemandLineWork = "ux_demand_line_work";
        public const string UxDemandLineHead = "ux_demand_line_head";
        /// <summary>T-27 partial UK: one sanction only against an AM&amp;R work (TSN-007, BUD-274, M-06).</summary>
        public const string UxTechnicalSanctionAmrWork = "ux_technical_sanction_amr_work";
        /// <summary>T-25 UK: one approval per work (a revision is a version, not a second approval).</summary>
        public const string UxAdminApprovalWork = "ux_admin_approval_work";
        /// <summary>T-25 UK (Division, serial).</summary>
        public const string UxAdminApprovalSerial = "ux_admin_approval_division_serial";
        /// <summary>T-27 UK on the estimate code (DB-05, TSN-009).</summary>
        public const string UxTechnicalSanctionEstimateCode = "ux_technical_sanction_estimate_code";

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
                // DV-07: a move outside the model cannot be written (BUD-235). Added by BudgetP1StateTransitionSeed.
                e.HasOne<StateTransition>().WithMany()
                    .HasForeignKey(x => new { x.FromState, x.ActionCode, x.ToState })
                    .HasConstraintName("fk_proposal_movement_state_transition")
                    .OnDelete(DeleteBehavior.Restrict);
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

            // ---------------- T-22 attachment (append-only) ----------------
            modelBuilder.Entity<BudgetAttachment>(e =>
            {
                e.Property(x => x.AttachmentId).UseIdentityAlwaysColumn();
                e.Property(x => x.Sha256Digest).IsFixedLength();
                e.HasIndex(x => x.StorageKey).IsUnique();
                ConfigureActorColumns(e);
            });

            // ---------------- T-19 delegation_limit (append-only) ----------------
            modelBuilder.Entity<DelegationLimit>(e =>
            {
                e.Property(x => x.DelegationLimitId).UseIdentityAlwaysColumn();
                e.Property(x => x.MaxAmount).HasColumnType(RupeesColumnType);
                e.HasIndex(x => new { x.AuthorityLevel, x.EffectiveFrom }).IsUnique();
                ConfigureActorColumns(e);
            });

            // ---------------- T-25 admin_approval ----------------
            modelBuilder.Entity<AdminApproval>(e =>
            {
                e.Property(x => x.AdminApprovalId).UseIdentityAlwaysColumn();
                e.HasIndex(x => x.WorkId).IsUnique().HasDatabaseName(UxAdminApprovalWork);
                e.HasIndex(x => new { x.DivisionOfficeId, x.SerialNo }).IsUnique().HasDatabaseName(UxAdminApprovalSerial);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.DivisionOfficeId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-26 admin_approval_version (append-only) ----------------
            modelBuilder.Entity<AdminApprovalVersion>(e =>
            {
                e.HasKey(x => new { x.AdminApprovalId, x.VersionNo });
                e.Property(x => x.AmountSanctioned).HasColumnType(RupeesColumnType);
                e.HasOne<AdminApproval>().WithMany().HasForeignKey(x => x.AdminApprovalId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.AaCopyAttachmentId).OnDelete(DeleteBehavior.Restrict);
                // account_code_id → budget_head_master(account_code_id) is added by SQL (unique index, not the PK).
                ConfigureActorColumns(e);
            });

            // ---------------- T-27 technical_sanction ----------------
            modelBuilder.Entity<TechnicalSanction>(e =>
            {
                e.Property(x => x.TechnicalSanctionId).UseIdentityAlwaysColumn();
                e.HasIndex(x => x.WorkId, "IX_technical_sanction_work_id");
                e.HasIndex(x => x.AdminApprovalId);
                e.HasIndex(x => x.WorkId, UxTechnicalSanctionAmrWork).IsUnique().HasFilter("tab = 'AMR'");
                e.HasIndex(x => x.EstimateCode).IsUnique().HasDatabaseName(UxTechnicalSanctionEstimateCode);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<AdminApproval>().WithMany().HasForeignKey(x => x.AdminApprovalId).OnDelete(DeleteBehavior.Restrict);
                // Written only by budget.fn_record_estimate_code (DB-05): EF never writes them.
                ReadOnly(e.Property(x => x.EstimateCode));
                ReadOnly(e.Property(x => x.EstimateCodeAt));
                ConfigureActorColumns(e);
            });

            // ---------------- T-28 technical_sanction_version (append-only) ----------------
            modelBuilder.Entity<TechnicalSanctionVersion>(e =>
            {
                e.HasKey(x => new { x.TechnicalSanctionId, x.VersionNo });
                e.Property(x => x.TsAmount).HasColumnType(RupeesColumnType);
                e.HasOne<TechnicalSanction>().WithMany().HasForeignKey(x => x.TechnicalSanctionId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.TsCopyAttachmentId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-45 correction_flag (provision_id P4, cleared_by_reappropriation_id P5) ----------------
            modelBuilder.Entity<CorrectionFlag>(e =>
            {
                e.Property(x => x.CorrectionFlagId).UseIdentityAlwaysColumn();
                e.Property(x => x.FigureAllocated).HasColumnType(RupeesColumnType);
                e.Property(x => x.FigureSupported).HasColumnType(RupeesColumnType);
                e.Property(x => x.IsCritical).HasDefaultValue(false);
                e.HasIndex(x => x.WorkId);
                e.HasOne<Work>().WithMany().HasForeignKey(x => x.WorkId).OnDelete(DeleteBehavior.Restrict);
                // cleared_by → employee.employee is added by SQL (cross-schema).
                ConfigureActorColumns(e);
            });

            // ---------------- T-30 attachment references (P2, DB-08 by trigger) ----------------
            modelBuilder.Entity<DemandLine>(e =>
            {
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.JustificationAttachmentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.CeCertificateAttachmentId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.CeAmrApprovalAttachmentId).OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- T-34 upload_batch (append-only, P2; F-08, BUD-101) ----------------
            modelBuilder.Entity<UploadBatch>(e =>
            {
                e.Property(x => x.UploadBatchId).UseIdentityAlwaysColumn();
                e.HasOne<BudgetProposal>().WithMany().HasForeignKey(x => x.ProposalId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<BudgetAttachment>().WithMany().HasForeignKey(x => x.FileAttachmentId).OnDelete(DeleteBehavior.Restrict);
                ConfigureActorColumns(e);
            });

            // ---------------- T-30 upload_batch reference (P2, BUD-101) ----------------
            modelBuilder.Entity<DemandLine>(e =>
            {
                e.HasOne<UploadBatch>().WithMany().HasForeignKey(x => x.UploadBatchId).OnDelete(DeleteBehavior.Restrict);
            });

            // ---------------- Views (design §5.4) ----------------
            modelBuilder.Entity<DemandLineDisplayRow>(e =>
            {
                e.HasNoKey().ToView("v_demand_line_display", "budget");
                e.Property(x => x.LevelFigures).HasColumnType("jsonb");
            });
            modelBuilder.Entity<DemandByCeRow>(e => e.HasNoKey().ToView("v_demand_by_ce", "budget"));
            modelBuilder.Entity<ManualOfAccountsCodeRow>(e => e.HasNoKey().ToView("v_manual_of_accounts_codes", "budget"));
            // ---------------- T-21 account_head_assignment (P2) ----------------
            modelBuilder.Entity<AccountHeadAssignment>(e =>
            {
                e.Property(x => x.AssignmentId).UseIdentityAlwaysColumn();
                // One live row per office and head; a removed row stays (DB-02, removal by marking).
                e.HasIndex(x => new { x.OfficeId, x.AccountCodeId }).IsUnique()
                    .HasDatabaseName(UxAccountHeadAssignmentLive).HasFilter("removed_at IS NULL");
                e.HasIndex(x => x.AccountCodeId);
                e.HasOne<BudgetOffice>().WithMany().HasForeignKey(x => x.OfficeId).OnDelete(DeleteBehavior.Restrict);
                // account_code_id → budget_head_master(account_code_id) and removed_by → employee are added by SQL.
                ConfigureActorColumns(e);
            });

            // ---------------- T-16 state_transition (seeded, design §6.2) ----------------
            modelBuilder.Entity<StateTransition>(e =>
            {
                e.HasKey(x => new { x.FromState, x.ActionCode, x.ToState });
                e.HasOne<ProposalState>().WithMany().HasForeignKey(x => x.FromState).OnDelete(DeleteBehavior.Restrict);
                e.HasOne<ProposalState>().WithMany().HasForeignKey(x => x.ToState).OnDelete(DeleteBehavior.Restrict);
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
