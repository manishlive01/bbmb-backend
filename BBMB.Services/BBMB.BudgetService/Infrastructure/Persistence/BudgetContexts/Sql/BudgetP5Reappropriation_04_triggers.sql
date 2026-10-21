-- BudgetP5Reappropriation step 4: trigger attachment (design §11.2). T-46 and T-47 are mutable ([A]), so they
-- take fn_require_actor, fn_audit_row and the statement-level DELETE/TRUNCATE guard, but no row-level
-- fn_forbid_change. The DB-24 rule triggers and the VR-08 balance checks of step 3 are attached last.

-- ===================== T-46 reappropriation (mutable) =====================
CREATE TRIGGER trg_reappropriation_require_actor BEFORE INSERT OR UPDATE ON budget.reappropriation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_reappropriation_audit AFTER INSERT OR UPDATE ON budget.reappropriation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_reappropriation_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.reappropriation
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-24: status moves from OPEN once; destination/amount fixed thereafter.
CREATE TRIGGER trg_reappropriation_status_once BEFORE UPDATE ON budget.reappropriation
    FOR EACH ROW EXECUTE FUNCTION budget.fn_reappropriation_status_once();
-- VR-08 (M-18) on submission, at commit.
CREATE CONSTRAINT TRIGGER trg_reappropriation_submit_balance AFTER UPDATE ON budget.reappropriation
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_reappropriation_submit_balance();

-- ===================== T-47 reappropriation_source (mutable while OPEN) =====================
CREATE TRIGGER trg_reappropriation_source_require_actor BEFORE INSERT OR UPDATE ON budget.reappropriation_source
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_reappropriation_source_audit AFTER INSERT OR UPDATE ON budget.reappropriation_source
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_reappropriation_source_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.reappropriation_source
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- DB-24 + §5.5 row c: OPEN-only edits, not the destination, is_removed one-way.
CREATE TRIGGER trg_reappropriation_source_rules BEFORE INSERT OR UPDATE ON budget.reappropriation_source
    FOR EACH ROW EXECUTE FUNCTION budget.fn_reappropriation_source_rules();
-- DB-24, VR-08 (M-18): cumulative ≤ additional, = on SUBMITTED, at commit.
CREATE CONSTRAINT TRIGGER trg_reappropriation_balance AFTER INSERT OR UPDATE ON budget.reappropriation_source
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_reappropriation_balance();
