-- BudgetP1ReferenceStateAndAudit step 4: the three functions of DDD §6.3 (DB-13, DB-27, DB-28, DB-29),
-- as specified in design §4.3. Attached by explicit table list only (design §11.2), never schema-wide.

-- ---------------------------------------------------------------------------------------------------
-- budget.fn_require_actor(): BEFORE INSERT OR UPDATE, row level, on every table of design §11.2.
-- Refuses the write where the actor context is absent or the charge is not a current charge of the
-- employee (DB-28). Fills only the actor columns the table has; on UPDATE it restores created_* to their
-- OLD values, so a creation stamp can never be altered. The application never sends them.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_require_actor() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_uuid  constant text := '^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$';
    v_emp   text := coalesce(current_setting('budget.actor_employee_id', true), '');
    v_chg   text := coalesce(current_setting('budget.actor_charge_id', true), '');
    v_now   timestamptz := clock_timestamp();
    v_row   jsonb := to_jsonb(NEW);
    v_old   jsonb;
    v_patch jsonb := '{}'::jsonb;
BEGIN
    IF v_emp !~ v_uuid OR v_chg !~ v_uuid THEN
        RAISE EXCEPTION 'ACTOR' USING ERRCODE = 'P0001',
            DETAIL = format('no actor context for %s on %I.%I (DB-28)', TG_OP, TG_TABLE_SCHEMA, TG_TABLE_NAME);
    END IF;

    -- is_charge_expire is set lazily by the application (D-12): the time test is repeated here.
    IF NOT EXISTS (
        SELECT 1
        FROM employee.employee_charge c
        WHERE c.id = v_chg::uuid
          AND c.employee_id = v_emp::uuid
          AND c.is_active
          AND NOT c.is_charge_expire
          AND (c.charge_expire_time IS NULL OR c.charge_expire_time > now())
    ) THEN
        RAISE EXCEPTION 'ACTOR' USING ERRCODE = 'P0001',
            DETAIL = format('charge is not a current charge of the employee, %s on %I.%I (DB-28)',
                            TG_OP, TG_TABLE_SCHEMA, TG_TABLE_NAME);
    END IF;

    IF TG_OP = 'INSERT' THEN
        IF v_row ? 'created_at' THEN
            v_patch := v_patch || jsonb_build_object(
                'created_at', v_now, 'created_by', v_emp, 'created_charge_id', v_chg);
        END IF;
    ELSE
        v_old := to_jsonb(OLD);
        IF v_row ? 'updated_at' THEN
            v_patch := v_patch || jsonb_build_object(
                'updated_at', v_now, 'updated_by', v_emp, 'updated_charge_id', v_chg);
        END IF;
        IF v_row ? 'created_at' THEN
            v_patch := v_patch || jsonb_build_object(
                'created_at', v_old -> 'created_at',
                'created_by', v_old -> 'created_by',
                'created_charge_id', v_old -> 'created_charge_id');
        END IF;
    END IF;

    -- T-07 budget_head_master keeps its existing modification_date (design §4.3); user_code stays the
    -- handler's responsibility.
    IF TG_TABLE_NAME = 'budget_head_master' AND v_row ? 'modification_date' THEN
        v_patch := v_patch || jsonb_build_object('modification_date', v_now);
    END IF;

    IF v_patch <> '{}'::jsonb THEN
        NEW := jsonb_populate_record(NEW, v_patch);
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- budget.fn_audit_row(): AFTER INSERT OR UPDATE, row level, SECURITY DEFINER, on the same list except
-- T-51 (DDD §6.3, DB-29). An insert writes one T-51 row holding the whole new row; an update writes one
-- row per changed column with old and new values (BUD-034, DDD §7.2). Runs in the transaction of the
-- change, so the change and its audit entry commit together or not at all.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_audit_row() RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_new    jsonb := to_jsonb(NEW);
    v_old    jsonb;
    v_key    text;
    v_col    text;
    v_val    jsonb;
    v_action text := nullif(current_setting('budget.action_code', true), '');
    v_emp    uuid := nullif(current_setting('budget.actor_employee_id', true), '')::uuid;
    v_name   text := nullif(current_setting('budget.actor_name', true), '');
    v_chg    uuid := nullif(current_setting('budget.actor_charge_id', true), '')::uuid;
    v_office bigint := nullif(current_setting('budget.actor_office_id', true), '')::bigint;
    v_role   text := nullif(current_setting('budget.actor_role_code', true), '');
    v_obo    text := nullif(current_setting('budget.on_behalf_of_post', true), '');
    v_ip     inet := nullif(current_setting('budget.client_ip', true), '')::inet;
    v_req    uuid := nullif(current_setting('budget.request_id', true), '')::uuid;
BEGIN
    -- record_key: the primary key of the row, as text (composite keys joined by '|').
    SELECT string_agg(v_new ->> a.attname, '|' ORDER BY array_position(i.indkey::int2[], a.attnum))
    INTO v_key
    FROM pg_index i
    JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY (i.indkey)
    WHERE i.indrelid = TG_RELID AND i.indisprimary;

    IF TG_OP = 'INSERT' THEN
        INSERT INTO budget.audit_log (table_name, record_key, operation, column_name, old_value, new_value,
                                      action_code, actor_employee_id, actor_name, actor_charge_id,
                                      actor_office_id, actor_role_code, on_behalf_of_post, client_ip, request_id)
        VALUES (TG_TABLE_NAME, coalesce(v_key, ''), 'I', NULL, NULL, v_new,
                v_action, v_emp, v_name, v_chg, v_office, v_role, v_obo, v_ip, v_req);
    ELSE
        v_old := to_jsonb(OLD);
        FOR v_col, v_val IN SELECT key, value FROM jsonb_each(v_new) LOOP
            IF (v_old -> v_col) IS DISTINCT FROM v_val THEN
                INSERT INTO budget.audit_log (table_name, record_key, operation, column_name, old_value, new_value,
                                              action_code, actor_employee_id, actor_name, actor_charge_id,
                                              actor_office_id, actor_role_code, on_behalf_of_post, client_ip, request_id)
                VALUES (TG_TABLE_NAME, coalesce(v_key, ''), 'U', v_col, v_old -> v_col, v_val,
                        v_action, v_emp, v_name, v_chg, v_office, v_role, v_obo, v_ip, v_req);
            END IF;
        END LOOP;
    END IF;
    RETURN NULL;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- budget.fn_forbid_change(): BEFORE UPDATE OR DELETE (row) on append-only tables, and BEFORE DELETE OR
-- TRUNCATE (statement) on every table of design §11.2 (DDD §6.3; DB-13, DB-27). Refuses whoever acts,
-- the table owner included. A row-level trigger does not fire on TRUNCATE, hence the statement level.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_forbid_change() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    RAISE EXCEPTION 'FORBIDDEN' USING ERRCODE = 'P0001',
        DETAIL = format('%s on %I.%I is not permitted (DB-13, DB-27)', TG_OP, TG_TABLE_SCHEMA, TG_TABLE_NAME);
END
$fn$;
