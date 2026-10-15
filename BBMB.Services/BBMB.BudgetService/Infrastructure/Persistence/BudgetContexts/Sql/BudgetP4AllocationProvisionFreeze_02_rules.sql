-- BudgetP4AllocationProvisionFreeze step 2: DDD §5.7, §5.8 CHECKs, the account-code and cross-schema actor
-- references EF does not create, and the T-41 live-holder partial unique index. Not gated: this migration
-- carries no DDD deviation awaiting a decision (§11.4). It writes no data, so the migration actor is not
-- needed (§11.1 rule 8).

-- =====================================================================================================
-- T-40 allocation (append-only; [C]). body in (BSC, FULL_BOARD); total_activated >= 0 (DB-20 shape).
-- =====================================================================================================
ALTER TABLE budget.allocation
    ADD CONSTRAINT ck_allocation_body CHECK (body IN ('BSC', 'FULL_BOARD')),
    ADD CONSTRAINT ck_allocation_total CHECK (total_activated >= 0),
    ADD CONSTRAINT fk_allocation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_allocation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-41 provision (written once except supersession; [C]). basis in (RBE, OBE); holder_type in
-- (WORK, HEAD, UNFORESEEN); work_id not null exactly for WORK (DB-04); account_code_id not null for WORK and
-- HEAD; effective_from = 1 April of fin_year for OBE (BUD-182); budgetary_check false exactly for RECEIPTS
-- (BUD-307). The six-segment head reference is the account_code_id FK to T-07.
-- =====================================================================================================
ALTER TABLE budget.provision
    ADD CONSTRAINT ck_provision_basis CHECK (basis IN ('RBE', 'OBE')),
    ADD CONSTRAINT ck_provision_category CHECK (category IN ('WORKS', 'DNA', 'RECEIPTS', 'CAPEX')),
    ADD CONSTRAINT ck_provision_holder_type CHECK (holder_type IN ('WORK', 'HEAD', 'UNFORESEEN')),
    ADD CONSTRAINT ck_provision_work_id CHECK ((holder_type = 'WORK') = (work_id IS NOT NULL)),
    ADD CONSTRAINT ck_provision_account_code CHECK (holder_type = 'UNFORESEEN' OR account_code_id IS NOT NULL),
    ADD CONSTRAINT ck_provision_effective_from CHECK (
        basis = 'RBE'
        OR effective_from = make_date((fin_year - 1), 4, 1)),
    ADD CONSTRAINT ck_provision_budgetary_check CHECK ((category = 'RECEIPTS') = (NOT budgetary_check)),
    ADD CONSTRAINT fk_provision_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_provision_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_provision_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- A partial unique index on the live-holder key prevents two live provisions for the same holder and basis
-- (DDD §5.7). It is filtered on the self-FK superseded_by_provision_id, so it is a SQL object, not an EF index.
CREATE UNIQUE INDEX ux_provision_live_holder ON budget.provision
    (fin_year, basis, holder_type, work_id, office_id, account_code_id)
    WHERE superseded_by_provision_id IS NULL;

-- =====================================================================================================
-- T-42 provision_movement (ledger, append-only; [C]). amount signed per movement_type; exactly one reference
-- column populated per type (DDD §5.7). reappropriation_id's FK to T-46 is added in P5.
-- =====================================================================================================
ALTER TABLE budget.provision_movement
    ADD CONSTRAINT ck_provision_movement_type CHECK (movement_type IN ('ALLOCATION', 'REAPPROP_IN', 'REAPPROP_OUT', 'UNFORESEEN_IN', 'UNFORESEEN_OUT')),
    ADD CONSTRAINT ck_provision_movement_sign CHECK (
        (movement_type IN ('ALLOCATION', 'REAPPROP_IN', 'UNFORESEEN_IN') AND amount > 0)
        OR (movement_type IN ('REAPPROP_OUT', 'UNFORESEEN_OUT') AND amount < 0)),
    ADD CONSTRAINT ck_provision_movement_allocation_ref CHECK ((movement_type = 'ALLOCATION') = (allocation_id IS NOT NULL)),
    ADD CONSTRAINT ck_provision_movement_reappropriation_ref CHECK ((movement_type IN ('REAPPROP_IN', 'REAPPROP_OUT')) = (reappropriation_id IS NOT NULL)),
    ADD CONSTRAINT ck_provision_movement_unforeseen_ref CHECK ((movement_type IN ('UNFORESEEN_IN', 'UNFORESEEN_OUT')) = (unforeseen_authorisation_id IS NOT NULL)),
    ADD CONSTRAINT fk_provision_movement_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_provision_movement_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-43 estimate_allocation (mutable; [A]). amount >= 0 (whole rupees by the domain); DB-23 at commit below.
-- =====================================================================================================
ALTER TABLE budget.estimate_allocation
    ADD CONSTRAINT ck_estimate_allocation_amount CHECK (amount >= 0),
    ADD CONSTRAINT fk_estimate_allocation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_estimate_allocation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_estimate_allocation_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_estimate_allocation_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-44 freeze (append-only; [C]).
-- =====================================================================================================
ALTER TABLE budget.freeze
    ADD CONSTRAINT fk_freeze_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_freeze_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-48 unforeseen_request (mutable; [A]). status enumerated; at least one amount with its provision (DDD §5.8).
-- =====================================================================================================
ALTER TABLE budget.unforeseen_request
    ADD CONSTRAINT ck_unforeseen_request_status CHECK (status IN ('OPEN', 'AT_CIRCLE', 'AT_CE', 'RETURNED', 'AUTHORISED')),
    ADD CONSTRAINT ck_unforeseen_request_current_pair CHECK ((amount_current IS NULL) = (current_provision_id IS NULL)),
    ADD CONSTRAINT ck_unforeseen_request_next_pair CHECK ((amount_next IS NULL) = (next_provision_id IS NULL)),
    ADD CONSTRAINT ck_unforeseen_request_amount CHECK (current_provision_id IS NOT NULL OR next_provision_id IS NOT NULL),
    ADD CONSTRAINT fk_unforeseen_request_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_unforeseen_request_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_unforeseen_request_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_unforeseen_request_updated_charge FOREIGN KEY (updated_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-49 unforeseen_request_movement (append-only; [C]). from/to status enumerated; the permitted pairs and
-- the remark-on-return rule are enforced by the status-move trigger (fn_unforeseen_movement_rules); a remark
-- is required on any return (DDD §5.8).
-- =====================================================================================================
ALTER TABLE budget.unforeseen_request_movement
    ADD CONSTRAINT ck_unforeseen_movement_from CHECK (from_status IN ('OPEN', 'AT_CIRCLE', 'AT_CE', 'RETURNED', 'AUTHORISED')),
    ADD CONSTRAINT ck_unforeseen_movement_to CHECK (to_status IN ('OPEN', 'AT_CIRCLE', 'AT_CE', 'RETURNED', 'AUTHORISED')),
    ADD CONSTRAINT ck_unforeseen_movement_return_remark CHECK (to_status <> 'RETURNED' OR (remark IS NOT NULL AND btrim(remark) <> '')),
    ADD CONSTRAINT fk_unforeseen_movement_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_unforeseen_movement_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-50 unforeseen_authorisation (append-only; [C]).
-- =====================================================================================================
ALTER TABLE budget.unforeseen_authorisation
    ADD CONSTRAINT fk_unforeseen_authorisation_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_unforeseen_authorisation_created_charge FOREIGN KEY (created_charge_id) REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- =====================================================================================================
-- T-45 correction_flag: the P4 provision_id makes "at least one of work and provision" enforceable (DDD T-45).
-- The P2 placeholder ck_correction_flag_subject (work_id IS NOT NULL) is replaced here, as its P2 comment
-- states (BudgetP2AttachmentsApprovalsSanctions_02_rules.sql).
-- =====================================================================================================
ALTER TABLE budget.correction_flag
    DROP CONSTRAINT ck_correction_flag_subject,
    ADD CONSTRAINT ck_correction_flag_subject CHECK (work_id IS NOT NULL OR provision_id IS NOT NULL);
