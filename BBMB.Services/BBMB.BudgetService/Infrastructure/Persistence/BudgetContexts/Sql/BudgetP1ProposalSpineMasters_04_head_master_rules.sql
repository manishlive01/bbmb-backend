-- BudgetP1ProposalSpineMasters step 4 (design §5.2 T-07 order): NOT NULL and the DDD T-07 CHECKs.
ALTER TABLE budget.budget_head_master
    ALTER COLUMN origin SET NOT NULL,
    ALTER COLUMN party_emp_required SET NOT NULL,
    -- party_emp_required: CHECK in Y, N.
    ADD CONSTRAINT ck_budget_head_master_party_emp CHECK (party_emp_required IN ('Y', 'N')),
    -- origin: CHECK in CREATED (at F-01), LEGACY (loaded from the Manual in use).
    ADD CONSTRAINT ck_budget_head_master_origin CHECK (origin IN ('CREATED', 'LEGACY')),
    -- eoffice_approval_ref: CHECK not null where origin is CREATED (BUD-044, M-03).
    ADD CONSTRAINT ck_budget_head_master_eoffice_ref CHECK (origin <> 'CREATED' OR eoffice_approval_ref IS NOT NULL),
    -- not_for_use_ref, _at, _by: set with the mark; _ref required with it.
    ADD CONSTRAINT ck_budget_head_master_not_for_use
        CHECK (NOT not_for_use OR (not_for_use_ref IS NOT NULL AND not_for_use_at IS NOT NULL AND not_for_use_by IS NOT NULL)),
    -- created_*: required where origin is CREATED (F-01 column 15).
    ADD CONSTRAINT ck_budget_head_master_created
        CHECK (origin <> 'CREATED' OR (created_at IS NOT NULL AND created_by IS NOT NULL AND created_charge_id IS NOT NULL)),
    ADD CONSTRAINT fk_budget_head_master_created_by FOREIGN KEY (created_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_budget_head_master_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_budget_head_master_not_for_use_by FOREIGN KEY (not_for_use_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT;

-- T-17 setting.account_code_id → T-07 (deferred from BudgetP1ReferenceStateAndAudit: the key did not exist).
ALTER TABLE budget.setting
    ADD CONSTRAINT fk_setting_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT;
