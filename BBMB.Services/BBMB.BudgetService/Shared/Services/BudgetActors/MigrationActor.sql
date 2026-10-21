-- MigrationActor.sql (design §11.1 rule 8, DDD §9): run first by every data-shaping migration step.
-- Acts under the named IT Cell officer executing the deployment, whose ids arrive on the migration
-- connection only, as start-up settings (Npgsql Options '-c bbmb_migration.actor_employee_id=<id>
-- -c bbmb_migration.actor_charge_id=<id>'). No synthetic or shared account exists.
-- Tokens replaced by SqlResource.MigrationActor(): {{MIGRATION}} (migration name), {{JUSTIFICATION}}
-- (a SQL expression: the gate value of a gated migration, else '').
DO $migration_actor$
DECLARE
    v_uuid   constant text := '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$';
    v_emp    text := coalesce(current_setting('bbmb_migration.actor_employee_id', true), '');
    v_chg    text := coalesce(current_setting('bbmb_migration.actor_charge_id', true), '');
    v_name   text;
    v_office text := '';
BEGIN
    -- A persisted value would silently attribute a later run to this officer (design §11.1 rule 8).
    IF EXISTS (
        SELECT 1
        FROM pg_db_role_setting s, unnest(s.setconfig) AS c(cfg)
        WHERE s.setdatabase IN (0, (SELECT oid FROM pg_database WHERE datname = current_database()))
          AND s.setrole IN (0, (SELECT oid FROM pg_roles WHERE rolname = current_user))
          AND c.cfg LIKE 'bbmb\_migration.%'
    ) THEN
        RAISE EXCEPTION 'migration actor set at database or role level';
    END IF;

    -- Same test as budget.fn_require_actor, including charge_expire_time (is_charge_expire is lazy, D-12).
    IF v_emp !~ v_uuid OR v_chg !~ v_uuid OR NOT EXISTS (
        SELECT 1
        FROM employee.employee_charge c
        WHERE c.id = v_chg::uuid
          AND c.employee_id = v_emp::uuid
          AND c.is_active
          AND NOT c.is_charge_expire
          AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
    ) THEN
        RAISE EXCEPTION 'migration actor absent or inactive';
    END IF;

    SELECT e.name_english INTO v_name FROM employee.employee e WHERE e.id = v_emp::uuid;

    -- The IT Cell office (DDD L-03). Absent only before BudgetP1ReferenceStateAndAudit bootstraps it; the
    -- bootstrap step then sets actor_office_id itself.
    IF to_regclass('budget.office') IS NOT NULL THEN
        EXECUTE 'SELECT coalesce(min(office_id)::text, '''') FROM budget.office WHERE office_level = ''IT_CELL'''
            INTO v_office;
    END IF;

    -- Every key is set (empty when it has no value), so nothing survives from an earlier step (§4.3, DV-06).
    PERFORM set_config('budget.actor_employee_id', v_emp, true),
            set_config('budget.actor_charge_id', v_chg, true),
            set_config('budget.actor_office_id', v_office, true),
            set_config('budget.actor_role_code', 'MIGRATION', true),
            set_config('budget.actor_name', coalesce(v_name, ''), true),
            set_config('budget.on_behalf_of_post', '', true),
            set_config('budget.client_ip', '0.0.0.0', true),
            set_config('budget.request_id', gen_random_uuid()::text, true),
            set_config('budget.action_code', 'MIGRATION:{{MIGRATION}}', true),
            set_config('budget.justification', {{JUSTIFICATION}}, true),
            set_config('budget.no_justification', '', true);
END
$migration_actor$;
