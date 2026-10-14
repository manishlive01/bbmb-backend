-- BudgetP1ProposalSpineMasters step 5 (design §5.2 work order, step 2; §11.2): actor and audit triggers
-- and the statement-level DELETE/TRUNCATE guard on work, before the audited backfill.
-- trg_work_pre_ddd_one_way is attached last (step 8), because the backfill is a false -> true update.
CREATE TRIGGER trg_work_require_actor BEFORE INSERT OR UPDATE ON budget.work
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_work_audit AFTER INSERT OR UPDATE ON budget.work
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_work_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.work
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
