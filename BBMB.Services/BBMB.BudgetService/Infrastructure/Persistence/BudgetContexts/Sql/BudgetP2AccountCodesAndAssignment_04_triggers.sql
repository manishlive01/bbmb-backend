-- BudgetP2AccountCodesAndAssignment step 4: T-21 CHECKs and cross-table FKs, and trigger attachment (design §11.2).
ALTER TABLE budget.account_head_assignment
    ADD CONSTRAINT ck_account_head_assignment_source CHECK (source IN ('R04_LOAD', 'AFA')),
    ADD CONSTRAINT ck_account_head_assignment_removed CHECK ((removed_at IS NULL) = (removed_by IS NULL)),
    ADD CONSTRAINT fk_account_head_assignment_account_code FOREIGN KEY (account_code_id)
        REFERENCES budget.budget_head_master (account_code_id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_account_head_assignment_removed_by FOREIGN KEY (removed_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_account_head_assignment_created_by FOREIGN KEY (created_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_account_head_assignment_created_charge FOREIGN KEY (created_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_account_head_assignment_updated_by FOREIGN KEY (updated_by) REFERENCES employee.employee (id) ON DELETE RESTRICT,
    ADD CONSTRAINT fk_account_head_assignment_updated_charge FOREIGN KEY (updated_charge_id)
        REFERENCES employee.employee_charge (id) ON DELETE RESTRICT;

-- T-21 account_head_assignment (mutable, [A]; removal by marking; DB-01, DB-02, DB-03).
CREATE TRIGGER trg_account_head_assignment_require_actor BEFORE INSERT OR UPDATE ON budget.account_head_assignment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_account_head_assignment_audit AFTER INSERT OR UPDATE ON budget.account_head_assignment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_account_head_assignment_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.account_head_assignment
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_account_head_assignment_rules BEFORE INSERT OR UPDATE ON budget.account_head_assignment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_account_head_assignment_rules();

-- T-07 budget_head_master: the column-limited update (DB-03).
CREATE TRIGGER trg_head_master_protect BEFORE UPDATE ON budget.budget_head_master
    FOR EACH ROW EXECUTE FUNCTION budget.fn_head_master_protect();

-- T-23 work: DB-03 on the six-column reference.
CREATE TRIGGER trg_work_head_not_for_use BEFORE INSERT OR UPDATE OF major_acc_code, sub_major_acc_code, minor_acc_code,
    sub_minor_acc_code, detail_acc_code, object_acc_code ON budget.work
    FOR EACH ROW EXECUTE FUNCTION budget.fn_work_head_not_for_use();

-- T-30 demand_line: DB-02 demand side.
CREATE TRIGGER trg_demand_line_head_assigned BEFORE INSERT OR UPDATE OF account_code_id ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_demand_line_head_assigned();
