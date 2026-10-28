-- BudgetAuthzConfigTables step 2 (design §1, §7; §11.2): the actor, audit and DELETE/TRUNCATE-forbid
-- triggers on every authz-config table, BEFORE the seed insert (step 4), so each seed row is itself audited
-- into budget.audit_log. Same three-trigger pattern as every mutable, audited budget table (e.g. T-13).
-- The config tables are mutable + audited: an upsert is an audited UPDATE; the append-only change history is
-- budget.audit_log, so no separate history table is created (design §1).

DO $authz_triggers$
DECLARE
    t text;
    tables text[] := ARRAY[
        'budget_project_settings',
        'budget_role',
        'budget_right',
        'budget_role_right',
        'budget_query_field',
        'budget_role_query_field',
        'budget_admin_role_assignment'
    ];
BEGIN
    FOREACH t IN ARRAY tables LOOP
        EXECUTE format(
            'CREATE TRIGGER trg_%1$s_require_actor BEFORE INSERT OR UPDATE ON budget.%1$s '
            || 'FOR EACH ROW EXECUTE FUNCTION budget.fn_require_actor();', t);
        EXECUTE format(
            'CREATE TRIGGER trg_%1$s_audit AFTER INSERT OR UPDATE ON budget.%1$s '
            || 'FOR EACH ROW EXECUTE FUNCTION budget.fn_audit_row();', t);
        EXECUTE format(
            'CREATE TRIGGER trg_%1$s_forbid_stmt BEFORE DELETE OR TRUNCATE ON budget.%1$s '
            || 'FOR EACH STATEMENT EXECUTE FUNCTION budget.fn_forbid_change();', t);
    END LOOP;
END
$authz_triggers$;
