-- BudgetP2AttachmentsApprovalsSanctions step 2: DDD T-19, T-22, T-25 to T-28 and T-45 CHECKs, and the
-- references EF does not create (employee schema; T-07 account_code_id, a unique index and not the PK).
-- Not gated: P2 carries no DDD deviation awaiting a decision (design §11.4). No data is written, so the
-- migration actor is not needed (design §11.1 rule 8).
-- T-22 budget.attachment.
ALTER TABLE budget.attachment
    ADD CONSTRAINT ck_attachment_extension CHECK (
        file_extension IN ('pdf', 'xml', 'xls', 'xlsx', 'doc', 'docx', 'jpg', 'jpeg', 'png', 'tif', 'tiff')),
    ADD CONSTRAINT ck_attachment_evidence_class CHECK (evidence_class IN ('EVIDENCE', 'SUPPORTING')),
    -- EVIDENCE admits only pdf and the image formats (BUD-019).
    ADD CONSTRAINT ck_attachment_evidence_format CHECK (
        evidence_class <> 'EVIDENCE' OR file_extension IN ('pdf', 'jpg', 'jpeg', 'png', 'tif', 'tiff')),
    -- "10 MB" of GEN-009 read as 10 x 1024 x 1024 bytes (NIC's design decision, DDD T-22).
    ADD CONSTRAINT ck_attachment_size CHECK (size_bytes > 0 AND size_bytes <= 10485760),
    ADD CONSTRAINT ck_attachment_digest CHECK (sha256_digest ~ '^[0-9a-f]{64}$'),
    ADD CONSTRAINT fk_attachment_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_attachment_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-19 budget.delegation_limit.
ALTER TABLE budget.delegation_limit
    ADD CONSTRAINT ck_delegation_limit_level CHECK (authority_level IN ('SECRETARY', 'CE', 'SE', 'EE')),
    -- Null means no limit; CHECK > 0 where set.
    ADD CONSTRAINT ck_delegation_limit_amount CHECK (max_amount IS NULL OR max_amount > 0),
    ADD CONSTRAINT fk_delegation_limit_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_delegation_limit_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-25 budget.admin_approval.
ALTER TABLE budget.admin_approval
    ADD CONSTRAINT ck_admin_approval_version CHECK (current_version_no >= 1),
    ADD CONSTRAINT fk_admin_approval_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_admin_approval_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_admin_approval_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_admin_approval_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-26 budget.admin_approval_version.
ALTER TABLE budget.admin_approval_version
    ADD CONSTRAINT ck_admin_approval_version_no CHECK (version_no >= 1),
    -- Column 4: not later than the IST date of created_at (BUD-014, BUD-056). created_at is filled by
    -- fn_require_actor in the BEFORE phase, so it is present when the CHECK is evaluated.
    ADD CONSTRAINT ck_admin_approval_version_date CHECK (
        date_of_approval <= (created_at AT TIME ZONE 'Asia/Kolkata')::date),
    ADD CONSTRAINT ck_admin_approval_version_amount CHECK (amount_sanctioned > 0),
    ADD CONSTRAINT ck_admin_approval_version_authority CHECK (competent_authority IN ('SECRETARY', 'CE', 'SE', 'EE')),
    -- M-27: a justification where the amount exceeds the delegated powers (BUD-055).
    ADD CONSTRAINT ck_admin_approval_version_justification CHECK (
        NOT exceeds_delegation OR btrim(coalesce(delegation_justification, '')) <> ''),
    ADD CONSTRAINT fk_admin_approval_version_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_admin_approval_version_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_admin_approval_version_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-27 budget.technical_sanction.
ALTER TABLE budget.technical_sanction
    ADD CONSTRAINT ck_technical_sanction_tab CHECK (tab IN ('AA', 'AMR')),
    -- admin_approval_id: not null exactly when the tab is AA (BUD-069, BUD-070).
    ADD CONSTRAINT ck_technical_sanction_approval CHECK ((admin_approval_id IS NOT NULL) = (tab = 'AA')),
    ADD CONSTRAINT ck_technical_sanction_version CHECK (current_version_no >= 1),
    ADD CONSTRAINT ck_technical_sanction_estimate_code CHECK (
        (estimate_code IS NULL) = (estimate_code_at IS NULL) AND (estimate_code IS NULL OR btrim(estimate_code) <> '')),
    ADD CONSTRAINT fk_technical_sanction_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_technical_sanction_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_technical_sanction_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_technical_sanction_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-28 budget.technical_sanction_version.
ALTER TABLE budget.technical_sanction_version
    ADD CONSTRAINT ck_technical_sanction_version_no CHECK (version_no >= 1),
    ADD CONSTRAINT ck_technical_sanction_version_date CHECK (
        ts_date <= (created_at AT TIME ZONE 'Asia/Kolkata')::date),
    ADD CONSTRAINT ck_technical_sanction_version_amount CHECK (ts_amount > 0),
    ADD CONSTRAINT fk_technical_sanction_version_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_technical_sanction_version_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
-- T-45 budget.correction_flag. Until P4 adds provision_id, "at least one of work and provision" reads as
-- work_id not null; BudgetP4AllocationProvisionFreeze replaces ck_correction_flag_subject.
ALTER TABLE budget.correction_flag
    ADD CONSTRAINT ck_correction_flag_reason CHECK (
        reason IN ('AA_REDUCED', 'AA_HEAD_CHANGED', 'PROPOSAL_REOPENED', 'REALLOCATE_ESTIMATES', 'BELOW_EXPENDITURE')),
    ADD CONSTRAINT ck_correction_flag_subject CHECK (work_id IS NOT NULL),
    ADD CONSTRAINT ck_correction_flag_cleared CHECK ((cleared_at IS NULL) = (cleared_by IS NULL)),
    ADD CONSTRAINT fk_correction_flag_cleared_by FOREIGN KEY (cleared_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_correction_flag_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_correction_flag_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;
