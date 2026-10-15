-- BudgetP2AttachmentsApprovalsSanctions step 4: trigger attachment (design §11.2) and the rule triggers.
-- Every new table: fn_require_actor, fn_audit_row, the statement-level DELETE/TRUNCATE guard.
-- Append-only (DB-27): T-19, T-22, T-26, T-28 also carry the row-level fn_forbid_change.
-- T-22 attachment (append-only).
CREATE TRIGGER trg_attachment_require_actor BEFORE INSERT OR UPDATE ON budget.attachment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_attachment_audit AFTER INSERT OR UPDATE ON budget.attachment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_attachment_forbid_row BEFORE UPDATE OR DELETE ON budget.attachment
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_attachment_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.attachment
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- T-19 delegation_limit (append-only).
CREATE TRIGGER trg_delegation_limit_require_actor BEFORE INSERT OR UPDATE ON budget.delegation_limit
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_delegation_limit_audit AFTER INSERT OR UPDATE ON budget.delegation_limit
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_delegation_limit_forbid_row BEFORE UPDATE OR DELETE ON budget.delegation_limit
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_delegation_limit_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.delegation_limit
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
-- T-25 admin_approval (mutable header: current_version_no only).
CREATE TRIGGER trg_admin_approval_require_actor BEFORE INSERT OR UPDATE ON budget.admin_approval
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_admin_approval_audit AFTER INSERT OR UPDATE ON budget.admin_approval
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_admin_approval_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.admin_approval
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_admin_approval_rules BEFORE INSERT OR UPDATE ON budget.admin_approval
    FOR EACH ROW EXECUTE FUNCTION budget.fn_admin_approval_rules();
CREATE CONSTRAINT TRIGGER trg_admin_approval_version_check AFTER INSERT OR UPDATE ON budget.admin_approval
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_admin_approval_version_check();
-- T-26 admin_approval_version (append-only).
CREATE TRIGGER trg_admin_approval_version_require_actor BEFORE INSERT OR UPDATE ON budget.admin_approval_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_admin_approval_version_audit AFTER INSERT OR UPDATE ON budget.admin_approval_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_admin_approval_version_forbid_row BEFORE UPDATE OR DELETE ON budget.admin_approval_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_admin_approval_version_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.admin_approval_version
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_admin_approval_version_rules BEFORE INSERT ON budget.admin_approval_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_admin_approval_version_rules();
CREATE CONSTRAINT TRIGGER trg_admin_approval_version_check AFTER INSERT ON budget.admin_approval_version
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_admin_approval_version_check();
-- T-27 technical_sanction (mutable header: current_version_no; estimate_code once, DB-05).
CREATE TRIGGER trg_technical_sanction_require_actor BEFORE INSERT OR UPDATE ON budget.technical_sanction
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_technical_sanction_audit AFTER INSERT OR UPDATE ON budget.technical_sanction
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_technical_sanction_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.technical_sanction
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_technical_sanction_rules BEFORE INSERT OR UPDATE ON budget.technical_sanction
    FOR EACH ROW EXECUTE FUNCTION budget.fn_technical_sanction_rules();
CREATE CONSTRAINT TRIGGER trg_technical_sanction_version_check AFTER INSERT OR UPDATE ON budget.technical_sanction
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_technical_sanction_version_check();
CREATE CONSTRAINT TRIGGER trg_technical_sanction_ts_ceiling AFTER UPDATE OF current_version_no ON budget.technical_sanction
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_ts_ceiling_check();
-- T-28 technical_sanction_version (append-only; DB-09 at commit).
CREATE TRIGGER trg_technical_sanction_version_require_actor BEFORE INSERT OR UPDATE ON budget.technical_sanction_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_technical_sanction_version_audit AFTER INSERT OR UPDATE ON budget.technical_sanction_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_technical_sanction_version_forbid_row BEFORE UPDATE OR DELETE ON budget.technical_sanction_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_technical_sanction_version_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.technical_sanction_version
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_technical_sanction_version_rules BEFORE INSERT ON budget.technical_sanction_version
    FOR EACH ROW EXECUTE FUNCTION budget.fn_technical_sanction_version_rules();
CREATE CONSTRAINT TRIGGER trg_technical_sanction_version_check AFTER INSERT ON budget.technical_sanction_version
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_technical_sanction_version_check();
CREATE CONSTRAINT TRIGGER trg_technical_sanction_version_ts_ceiling AFTER INSERT ON budget.technical_sanction_version
    DEFERRABLE INITIALLY DEFERRED
    FOR EACH ROW EXECUTE FUNCTION budget.fn_ts_ceiling_check();
-- T-45 correction_flag (clearance once, DB-05).
CREATE TRIGGER trg_correction_flag_require_actor BEFORE INSERT OR UPDATE ON budget.correction_flag
    FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
CREATE TRIGGER trg_correction_flag_audit AFTER INSERT OR UPDATE ON budget.correction_flag
    FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
CREATE TRIGGER trg_correction_flag_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.correction_flag
    FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();
CREATE TRIGGER trg_correction_flag_rules BEFORE INSERT OR UPDATE ON budget.correction_flag
    FOR EACH ROW EXECUTE FUNCTION budget.fn_correction_flag_rules();
-- T-30 demand_line: DB-08 on the two Chief Engineer files (the T-22 FKs are created by EF in this migration).
CREATE TRIGGER trg_demand_line_attachment_rules BEFORE INSERT OR UPDATE ON budget.demand_line
    FOR EACH ROW EXECUTE FUNCTION budget.fn_demand_line_attachment_rules();
