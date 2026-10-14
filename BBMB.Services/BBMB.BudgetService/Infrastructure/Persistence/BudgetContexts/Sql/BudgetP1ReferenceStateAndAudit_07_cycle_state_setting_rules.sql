-- BudgetP1ReferenceStateAndAudit step 7: T-14, T-15, T-17 rules (DDD T-14, T-15, T-17) and their
-- triggers (design §11.2).

-- T-14 budget.budget_cycle.
ALTER TABLE budget.budget_cycle
    ADD CONSTRAINT ck_budget_cycle_next_year CHECK (next_fin_year = current_fin_year + 1),
    ADD CONSTRAINT ck_budget_cycle_status CHECK (status IN ('OPEN', 'FROZEN', 'CLOSED')),
    ADD CONSTRAINT fk_budget_cycle_created_by FOREIGN KEY (created_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_budget_cycle_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_budget_cycle_updated_by FOREIGN KEY (updated_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_budget_cycle_updated_charge FOREIGN KEY (updated_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

CREATE TRIGGER trg_budget_cycle_require_actor BEFORE INSERT OR UPDATE ON budget.budget_cycle
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_budget_cycle_audit AFTER INSERT OR UPDATE ON budget.budget_cycle
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_budget_cycle_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.budget_cycle
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- T-15 budget.state: append-only reference data; no actor columns, so the actor trigger only checks the context.
ALTER TABLE budget.state
    ADD CONSTRAINT ck_state_code CHECK (state_code ~ '^S-(0[1-9]|1[0-6])$');

CREATE TRIGGER trg_state_require_actor BEFORE INSERT OR UPDATE ON budget.state
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_state_audit AFTER INSERT OR UPDATE ON budget.state
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_state_forbid_row BEFORE UPDATE OR DELETE ON budget.state
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_state_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.state
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

-- T-17 budget.setting: append-only, [C]. The FK account_code_id → T-07 is added by
-- BudgetP1ProposalSpineMasters, which creates budget_head_master.account_code_id.
ALTER TABLE budget.setting
    ADD CONSTRAINT ck_setting_key CHECK (setting_key IN ('TS_CEILING_PCT', 'NO_AA_LIMIT_RUPEES',
        'AA_DEMAND_CEILING_PCT', 'AMR_INCREASE_PCT', 'UPLOAD_MAX_LINES', 'UNFORESEEN_ACCOUNT_CODE')),
    -- numeric_value: for every key except UNFORESEEN_ACCOUNT_CODE.
    ADD CONSTRAINT ck_setting_numeric_value
        CHECK ((numeric_value IS NOT NULL) = (setting_key <> 'UNFORESEEN_ACCOUNT_CODE')),
    -- account_code_id: for UNFORESEEN_ACCOUNT_CODE only; null there means no head set (BUD-277).
    ADD CONSTRAINT ck_setting_account_code
        CHECK (setting_key = 'UNFORESEEN_ACCOUNT_CODE' OR account_code_id IS NULL),
    ADD CONSTRAINT fk_setting_created_by FOREIGN KEY (created_by)
        REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_setting_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

CREATE TRIGGER trg_setting_require_actor BEFORE INSERT OR UPDATE ON budget.setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_setting_audit AFTER INSERT OR UPDATE ON budget.setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_setting_forbid_row BEFORE UPDATE OR DELETE ON budget.setting
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_setting_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.setting
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
