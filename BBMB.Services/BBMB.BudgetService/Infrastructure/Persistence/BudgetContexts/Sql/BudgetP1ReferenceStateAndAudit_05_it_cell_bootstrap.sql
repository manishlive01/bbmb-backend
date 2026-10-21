-- BudgetP1ReferenceStateAndAudit step 5, design §11.1 rule 8 (last bullet), DDD §9 L-03.
-- T-51 actor_office_id is NOT NULL FK T-13, so the IT Cell row must exist before the first audited write.
-- Order: take the IT Cell office id from the identity sequence; set it as the actor's office (the
-- migration-actor fragment has run just before this step and set every other key); attach the actor and
-- audit triggers to T-13 and the actor trigger to T-51; insert the row. It is thereby audited with itself
-- as the actor's office. load-from-hierarchy later finds it and does not create a second.
DO $bootstrap$
DECLARE
    v bigint := nextval(pg_get_serial_sequence('budget.office', 'office_id'));
BEGIN
    PERFORM set_config('budget.actor_office_id', v::text, true);

    -- T-13: mutable, [A]. Actor + audit (row), DELETE/TRUNCATE guard (statement). Design §11.2.
    CREATE TRIGGER trg_office_require_actor BEFORE INSERT OR UPDATE ON budget.office
        FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
    CREATE TRIGGER trg_office_audit AFTER INSERT OR UPDATE ON budget.office
        FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();
    CREATE TRIGGER trg_office_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.office
        FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

    -- T-51: append-only, not audited (auditing the audit log would recurse). Actor check as a backstop
    -- against a direct insert without context; row and statement guards (DB-27, DV-05).
    CREATE TRIGGER trg_audit_log_require_actor BEFORE INSERT OR UPDATE ON budget.audit_log
        FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();
    CREATE TRIGGER trg_audit_log_forbid_row BEFORE UPDATE OR DELETE ON budget.audit_log
        FOR EACH ROW EXECUTE FUNCTION budget.fn_forbid_change();
    CREATE TRIGGER trg_audit_log_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.audit_log
        FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();

    INSERT INTO budget.office (office_id, office_level, office_name, parent_office_id, is_active)
        OVERRIDING SYSTEM VALUE
        VALUES (v, 'IT_CELL', 'IT Cell', NULL, true);
END
$bootstrap$;
