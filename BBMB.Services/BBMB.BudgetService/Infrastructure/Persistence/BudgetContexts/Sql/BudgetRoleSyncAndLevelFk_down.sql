-- BudgetRoleSyncAndLevelFk Down (development databases only, per the existing convention). Run after the
-- migration-actor fragment and before the new columns are dropped. The legacy office_level column was never
-- dropped (expand/contract) and is dual-written by the application, so it is normally already filled. Where it
-- is NULL it is restored from the levels row the FK points at, through the same alias list as Up (substituted
-- from BudgetRoleLevel.Aliases). The previous schema requires office_level NOT NULL; a role that still has no
-- level (synchronised and never configured) aborts Down with an EXCEPTION rather than being given an invented
-- level, which would grant an office the role never had. The CHECK ck_budget_role_office_level was never
-- removed. The FINANCE, SECRETARIAT and IT_CELL rows inserted into budget.levels by Up are left in place: other
-- data may reference them, and Up does not insert them twice.
DO $role_level_down$
DECLARE
    v_unmapped text;
BEGIN
    UPDATE budget.budget_role br
       SET office_level = a.code
      FROM budget.levels l
      JOIN ({{LEVEL_ALIASES}}) AS a(alias, code) ON a.alias = upper(btrim(l.name))
     WHERE l.id = br.office_level_id
       AND br.office_level IS NULL;
    SELECT string_agg(role_name, ', ' ORDER BY id) INTO v_unmapped FROM budget.budget_role WHERE office_level IS NULL;
    IF v_unmapped IS NOT NULL THEN
        RAISE EXCEPTION 'BudgetRoleSyncAndLevelFk Down: no office level for role(s) %; the previous schema requires one', v_unmapped;
    END IF;
END
$role_level_down$;
