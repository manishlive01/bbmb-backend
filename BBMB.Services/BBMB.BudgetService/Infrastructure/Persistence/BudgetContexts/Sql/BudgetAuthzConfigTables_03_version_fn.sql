-- BudgetAuthzConfigTables step 3 (design §2, OQ-1): the config version stamp.
-- One round-trip, monotonic. budget.fn_audit_row writes one budget.audit_log row (bare TG_TABLE_NAME in
-- table_name) for every INSERT/UPDATE on the seven authz-config tables; the max audit_id touching them is a
-- cheap, strictly increasing stamp. The config cache reads it to decide whether to rebuild the snapshot:
-- unchanged stamp => extend the TTL window without a rebuild; changed => rebuild (design §2).
-- STABLE: it does not modify the database; SECURITY INVOKER (the default) is sufficient — the caller already
-- reads budget.audit_log through the same connection.
CREATE OR REPLACE FUNCTION budget.fn_config_version()
    RETURNS bigint
    LANGUAGE sql
    STABLE
AS $$
    SELECT coalesce(max(audit_id), 0)::bigint
    FROM budget.audit_log
    WHERE table_name IN (
        'budget_project_settings',
        'budget_role',
        'budget_right',
        'budget_role_right',
        'budget_query_field',
        'budget_role_query_field',
        'budget_admin_role_assignment'
    );
$$;
