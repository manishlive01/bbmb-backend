-- BudgetP4WorkCodeConformance (design §11.2, "each phase's new tables"): work_code_serial (T-24) is a
-- process table. It is written only by fn_generate_work_code inside an allocation action scope, so it carries
-- the actor check and the audit trigger; last_serial increments (so it is NOT append-only — not in the
-- row-level fn_forbid_change list), and it is never deleted or truncated (statement-level guard).
CREATE TRIGGER trg_work_code_serial_require_actor BEFORE INSERT OR UPDATE ON budget.work_code_serial
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_work_code_serial_audit AFTER INSERT OR UPDATE ON budget.work_code_serial
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_work_code_serial_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.work_code_serial
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
