-- BudgetP1ProposalSpineMasters step 2 (design §5.2 T-07 order, §11.2): attach the actor and audit
-- triggers and the statement-level DELETE/TRUNCATE guard to T-07 before the backfills, so each value the
-- migration chooses has a T-51 row. trg_head_master_protect (column-limited UPDATE, DB-03) lands in P2.
CREATE TRIGGER trg_budget_head_master_require_actor BEFORE INSERT OR UPDATE ON budget.budget_head_master
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_budget_head_master_audit AFTER INSERT OR UPDATE ON budget.budget_head_master
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_budget_head_master_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.budget_head_master
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
