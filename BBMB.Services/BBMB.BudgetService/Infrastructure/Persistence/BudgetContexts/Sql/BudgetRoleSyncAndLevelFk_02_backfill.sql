-- BudgetRoleSyncAndLevelFk step 2 (D5): data shaping, run after the migration-actor fragment so every UPDATE
-- below is stamped by budget.fn_require_actor and audited by budget.fn_audit_row (the row-level triggers on
-- budget_role and budget_project_settings already cover the new columns: they audit the whole row).
--
-- 1. budget_role.office_level_id from the legacy office_level string (DIVISION, CIRCLE, CE, SECRETARIAT, FINANCE,
--    IT_CELL). A levels row matches when its name, trimmed and upper-cased, is an alias of that code. The alias
--    VALUES list below is substituted by the migration from BudgetRoleLevel.Aliases, the single mapping the
--    application uses, so SQL and code cannot disagree ('CE' -> CE; 'CR'/'CIRCLE' -> CIRCLE; 'DIV'/'DIVISION' ->
--    DIVISION; FINANCE, SECRETARIAT, IT_CELL by exact name). levels.status is NOT considered: it is the
--    hierarchy master's flag, false on every live row, and Budget authorisation does not read it. Where several
--    levels rows match, the lowest id wins. The legacy office_level string is left as it is (expand/contract:
--    the previous release still reads it).
--    A role whose level has no matching levels row keeps office_level_id NULL: it is unconfigured and denied
--    until a Budget administrator assigns a level (roles/configure). Each such role is reported in a WARNING.
--    ADMIN LOCKOUT GUARD: an ACTIVE role that is an administrator (is_admin, or holds an active Settings.Admin
--    grant) and finds no levels row would leave nobody able to open roles/configure to repair it. That aborts
--    the migration with an EXCEPTION, unless bbmb_gate.role_level_unmapped_admin is set (development and tests
--    only). Step 1 inserts FINANCE, SECRETARIAT and IT_CELL, so on the live data every role maps.
-- 2. budget_project_settings.sso_project_id: the Budget project's Auth SSO id, taken from real tokens
--    (ProjectName "Budget"). Set ONLY where it is still NULL and the name is 'Budget'; an administrator's value
--    is never overwritten.
DO $role_level$
DECLARE
    r                record;
    v_level_id       int;
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
        WHERE br.office_level_id IS NULL
        ORDER BY br.id
    LOOP
        SELECT l.id INTO v_level_id
        FROM budget.levels l
        JOIN ({{LEVEL_ALIASES}}) AS a(alias, code) ON a.alias = upper(btrim(l.name))
        WHERE a.code = upper(btrim(r.office_level))
        ORDER BY l.id
        LIMIT 1;
        IF v_level_id IS NULL THEN
            v_unmapped := v_unmapped || (r.role_name || '=' || coalesce(r.office_level, '<null>'));
            IF r.administers THEN
                v_admin_unmapped := v_admin_unmapped || (r.role_name || '=' || coalesce(r.office_level, '<null>'));
            END IF;
        ELSE
            UPDATE budget.budget_role SET office_level_id = v_level_id WHERE id = r.id;
        END IF;
    END LOOP;
    IF coalesce(array_length(v_admin_unmapped, 1), 0) > 0 THEN
        IF coalesce(current_setting('bbmb_gate.role_level_unmapped_admin', true), '') = '' THEN
            RAISE EXCEPTION 'BudgetRoleSyncAndLevelFk: no budget.levels row for administrator role(s) %; Budget administration would be locked out. Add or correct the budget.levels rows and re-run',
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
