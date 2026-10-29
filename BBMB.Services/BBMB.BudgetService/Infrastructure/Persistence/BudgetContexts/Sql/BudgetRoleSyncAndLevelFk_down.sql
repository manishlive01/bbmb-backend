-- BudgetRoleSyncAndLevelFk Down (development databases only, per the existing convention). Run after the
-- migration-actor fragment and after the legacy office_level column has been re-added (nullable). Restores the
-- string from the levels row the FK points at, where that name is a Budget office level code. A role with no
-- such level (office_level_id NULL, or a non-Budget level) is left NULL and reported rather than given an
-- invented level, which would grant an office the role never had. The CHECK constraint of
-- BudgetAuthzConfigTables is re-added; a CHECK passes on NULL, and the column stays nullable after Down.
DO $role_level_down$
DECLARE
    v_unmapped text;
BEGIN
    UPDATE budget.budget_role br
       SET office_level = upper(btrim(l.name))
      FROM budget.levels l
     WHERE l.id = br.office_level_id
       AND upper(btrim(l.name)) IN ('DIVISION','CIRCLE','CE','SECRETARIAT','FINANCE','IT_CELL');

    SELECT string_agg(role_name, ', ' ORDER BY id) INTO v_unmapped FROM budget.budget_role WHERE office_level IS NULL;
    IF v_unmapped IS NOT NULL THEN
        RAISE WARNING 'BudgetRoleSyncAndLevelFk Down: office_level left NULL for: %', v_unmapped;
    END IF;
END
$role_level_down$;
ALTER TABLE budget.budget_role
    ADD CONSTRAINT ck_budget_role_office_level
    CHECK (office_level IN ('DIVISION','CIRCLE','CE','SECRETARIAT','FINANCE','IT_CELL'));
