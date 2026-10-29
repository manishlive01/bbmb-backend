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
--    ADMIN LOCKOUT GUARD: an ACTIVE role that is an administrator (is_admin, or holds an active Settings.Admin
--    grant) and finds no levels row would leave nobody able to open roles/configure to repair it. That aborts
--    the migration with an EXCEPTION, unless bbmb_gate.role_level_unmapped_admin is set (development and the
--    Testcontainers tests, where budget.levels is empty, set it to 'TEST'). In a controlled environment, run
--    SELECT id, name, status FROM budget.levels first and make sure the FINANCE and IT_CELL rows exist by
--    those names; do not set the gate there.
-- 2. budget_project_settings.sso_project_id: the Budget project's Auth SSO id, taken from real tokens
--    (ProjectName "Budget"). Set ONLY where it is still NULL and the name is 'Budget'; an administrator's value
--    is never overwritten.
DO $role_level$
DECLARE
    r                record;
    v_level_id       int;
    v_level_active   boolean;
    v_unmapped       text[] := '{}';
    v_admin_unmapped text[] := '{}';
BEGIN
    FOR r IN
        SELECT br.id, br.role_name, br.office_level,
               br.is_active AND (br.is_admin OR EXISTS (
                   SELECT 1
                   FROM budget.budget_role_right rr
                   JOIN budget.budget_right rt ON rt.id = rr.right_id
                   WHERE rr.role_id = br.id AND rr.is_active AND rt.is_active AND rt.right_key = 'Settings.Admin'))
                   AS administers
        FROM budget.budget_role br
        ORDER BY br.id
    LOOP
        SELECT l.id, l.status INTO v_level_id, v_level_active
        FROM budget.levels l
        WHERE upper(btrim(l.name)) = upper(btrim(r.office_level))
        ORDER BY l.status DESC, l.id
        LIMIT 1;
        IF v_level_id IS NULL THEN
            v_unmapped := v_unmapped || (r.role_name || '=' || coalesce(r.office_level, '<null>'));
        ELSE
            UPDATE budget.budget_role SET office_level_id = v_level_id WHERE id = r.id;
        END IF;
        -- An inactive levels row does not authorise (BudgetRoleLevel.ToOfficeLevelCode), so for the guard it
        -- counts as unmapped.
        IF r.administers AND (v_level_id IS NULL OR NOT v_level_active) THEN
            v_admin_unmapped := v_admin_unmapped || (r.role_name || '=' || coalesce(r.office_level, '<null>'));
        END IF;
    END LOOP;

    IF coalesce(array_length(v_admin_unmapped, 1), 0) > 0 THEN
        IF coalesce(current_setting('bbmb_gate.role_level_unmapped_admin', true), '') = '' THEN
            RAISE EXCEPTION 'BudgetRoleSyncAndLevelFk: no active budget.levels row for administrator role(s) %; Budget administration would be locked out. Add or correct the budget.levels rows and re-run',
                array_to_string(v_admin_unmapped, ', ');
        END IF;
        RAISE WARNING 'BudgetRoleSyncAndLevelFk: gate role_level_unmapped_admin is set; administrator role(s) left unconfigured: %',
            array_to_string(v_admin_unmapped, ', ');
    END IF;

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
