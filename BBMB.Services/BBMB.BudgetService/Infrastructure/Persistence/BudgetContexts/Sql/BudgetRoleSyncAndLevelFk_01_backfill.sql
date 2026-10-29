-- BudgetRoleSyncAndLevelFk step 1 (D1, D3): data shaping, run after the migration-actor fragment so every
-- UPDATE below is stamped by budget.fn_require_actor and audited by budget.fn_audit_row (the row-level triggers
-- on budget_role and budget_project_settings already cover the new columns: they audit the whole row).
--
-- 1. budget_role.office_level_id: map the legacy office_level string (DIVISION, CIRCLE, CE, SECRETARIAT, FINANCE,
--    IT_CELL) to the EXISTING budget.levels master row whose name equals it, trimmed and case-insensitive — the
--    same rule as BudgetRoleLevel.ToOfficeLevelCode. Nothing is inserted into budget.levels. A role whose level
--    has no matching levels row keeps office_level_id NULL: it is unconfigured and denied until a Budget
--    administrator assigns a level (roles/configure). Each such role is reported in a WARNING, and the prior
--    office_level value stays recoverable from budget.audit_log (the seed INSERT recorded the whole row).
--    Where several levels rows share a name, an active row is preferred, then the lowest id.
-- 2. budget_project_settings.sso_project_id: the Budget project's Auth SSO id, taken from real tokens
--    (ProjectName "Budget"). Set ONLY where it is still NULL and the name is 'Budget'; an administrator's value
--    is never overwritten.
DO $role_level$
DECLARE
    r          record;
    v_level_id int;
    v_unmapped text[] := '{}';
BEGIN
    FOR r IN SELECT id, role_name, office_level FROM budget.budget_role ORDER BY id LOOP
        SELECT l.id INTO v_level_id
        FROM budget.levels l
        WHERE upper(btrim(l.name)) = upper(btrim(r.office_level))
        ORDER BY l.status DESC, l.id
        LIMIT 1;
        IF v_level_id IS NULL THEN
            v_unmapped := v_unmapped || (r.role_name || '=' || coalesce(r.office_level, '<null>'));
        ELSE
            UPDATE budget.budget_role SET office_level_id = v_level_id WHERE id = r.id;
        END IF;
    END LOOP;

    IF coalesce(array_length(v_unmapped, 1), 0) > 0 THEN
        RAISE WARNING 'BudgetRoleSyncAndLevelFk: no budget.levels row for % role(s); office_level_id left NULL, role unconfigured and denied: %',
            array_length(v_unmapped, 1), array_to_string(v_unmapped, ', ');
    END IF;

    UPDATE budget.budget_project_settings
       SET sso_project_id = 'fb9e91a1-3a86-4498-afb1-3549cba24b93'::uuid
     WHERE sso_project_id IS NULL
       AND sso_project_name = 'Budget';
END
$role_level$;
