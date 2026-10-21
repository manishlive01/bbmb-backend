-- BudgetP1ProposalSpine step 3: the database rules of the spine (DDD §6.1 DB-01, DB-03 reference side
-- on T-30, DB-04, DB-10 to DB-15). Each refusal raises P0001; where the rule has an SRS message number the
-- exception text is that number, so the global handler shows the catalogue text (design §4.9); the
-- others (DB-01, DB-04, DB-10, DB-12, DB-13, DB-15) are backstops behind application pre-checks.
-- DB-10 and DB-12 read T-16, created by BudgetP1StateTransitionSeed: PL/pgSQL resolves the table when the
-- function runs, so until then they refuse (design §11.4).

-- ---------------------------------------------------------------------------------------------------
-- T-29 on INSERT: DB-01 (WORKS only at a Division; the other three at a Division, Circle or CE office);
-- DB-10 (a proposal comes into being at S-01, held by its origin office; afterwards only a movement moves it).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_proposal_insert_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_level text;
BEGIN
    SELECT office_level INTO v_level FROM budget.office WHERE office_id = NEW.origin_office_id;
    IF (NEW.category = 'WORKS' AND v_level <> 'DIVISION')
       OR (NEW.category <> 'WORKS' AND v_level NOT IN ('DIVISION', 'CIRCLE', 'CE')) THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001',
            DETAIL = format('a %s proposal cannot originate at a %s office', NEW.category, v_level);
    END IF;
    IF NEW.state_code <> 'S-01' OR NEW.holder_office_id <> NEW.origin_office_id THEN
        RAISE EXCEPTION 'DB-10' USING ERRCODE = 'P0001',
            DETAIL = 'a proposal is created at S-01 held by its origin office';
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-29 on UPDATE: DB-01 (origin and category are fixed) and, at commit, DB-10: a change of state or holder
-- needs a movement row of this transaction whose from-state is the state before and whose to-state and
-- to-office are the state and holder after. Deferred, so the order in which the movement and the proposal
-- are written within the transaction does not matter.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_proposal_fixed_columns() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NEW.cycle_id <> OLD.cycle_id OR NEW.category <> OLD.category OR NEW.origin_office_id <> OLD.origin_office_id THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'cycle, category and origin of a proposal are fixed';
    END IF;
    RETURN NEW;
END
$fn$;

CREATE FUNCTION budget.fn_proposal_state_change() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NEW.state_code IS DISTINCT FROM OLD.state_code OR NEW.holder_office_id IS DISTINCT FROM OLD.holder_office_id THEN
        IF NOT EXISTS (
            SELECT 1 FROM budget.proposal_movement m
            WHERE m.proposal_id = NEW.proposal_id
              AND m.from_state = OLD.state_code
              AND m.to_state = NEW.state_code
              AND m.to_office_id = NEW.holder_office_id
              AND m.created_at >= now()   -- written in this transaction (now() is its start time)
        ) THEN
            RAISE EXCEPTION 'DB-10' USING ERRCODE = 'P0001',
                DETAIL = format('proposal %s moved %s -> %s without a movement row', NEW.proposal_id, OLD.state_code, NEW.state_code);
        END IF;
    END IF;
    RETURN NULL;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-33 on INSERT: DB-12. A movement requiring a remark carries one (T-16 remark_required, M-12); a RETURN
-- goes to the office immediately below the returning office (BUD-119, BUD-240, DV-07).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_movement_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_remark_required boolean;
BEGIN
    SELECT t.remark_required INTO v_remark_required
    FROM budget.state_transition t
    WHERE t.from_state = NEW.from_state AND t.action_code = NEW.action_code AND t.to_state = NEW.to_state;
    IF coalesce(v_remark_required, false) AND btrim(coalesce(NEW.remark, '')) = '' THEN
        RAISE EXCEPTION 'M-12' USING ERRCODE = 'P0001';
    END IF;
    IF NEW.action_code = 'RETURN' AND NOT EXISTS (
        SELECT 1 FROM budget.office o WHERE o.office_id = NEW.to_office_id AND o.parent_office_id = NEW.from_office_id
    ) THEN
        RAISE EXCEPTION 'DB-12' USING ERRCODE = 'P0001',
            DETAIL = 'a RETURN goes to the office immediately below the returning office';
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-30 on INSERT OR UPDATE: DB-01 (line type of the category), DB-03 (a code marked not for use is refused
-- as a new reference, M-05), DB-04 (a WORK line's work is of the proposal's Division), DB-11 (lines and
-- figures change only while the state has figures_open, at S-10 for WORKS only, by the holding office, and
-- never once the cycle is frozen or closed), DB-13 (is_deleted is one-way; deleted_at/by set with it).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_demand_line_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    p       record;
    v_actor bigint := nullif(current_setting('budget.actor_office_id', true), '')::bigint;
BEGIN
    SELECT pr.category, pr.state_code, pr.holder_office_id, s.figures_open, c.status AS cycle_status,
           o.division_id AS origin_division_id
    INTO p
    FROM budget.proposal pr
    JOIN budget.state s ON s.state_code = pr.state_code
    JOIN budget.budget_cycle c ON c.cycle_id = pr.cycle_id
    JOIN budget.office o ON o.office_id = pr.origin_office_id
    WHERE pr.proposal_id = NEW.proposal_id;

    -- DB-11
    IF p.cycle_status <> 'OPEN' THEN
        RAISE EXCEPTION 'M-17' USING ERRCODE = 'P0001';
    END IF;
    IF NOT p.figures_open OR (p.state_code = 'S-10' AND p.category <> 'WORKS') THEN
        RAISE EXCEPTION 'M-19' USING ERRCODE = 'P0001';
    END IF;
    IF v_actor IS DISTINCT FROM p.holder_office_id THEN
        RAISE EXCEPTION 'M-20' USING ERRCODE = 'P0001';
    END IF;

    IF TG_OP = 'UPDATE' THEN
        -- DB-13: the mark is one-way.
        IF OLD.is_deleted AND NOT NEW.is_deleted THEN
            RAISE EXCEPTION 'DB-13' USING ERRCODE = 'P0001', DETAIL = 'a deleted demand line cannot be restored';
        END IF;
        IF NEW.proposal_id <> OLD.proposal_id OR NEW.line_type <> OLD.line_type OR NEW.serial_no <> OLD.serial_no THEN
            RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'proposal, line type and serial of a line are fixed';
        END IF;
    END IF;
    IF NEW.is_deleted AND (TG_OP = 'INSERT' OR NOT OLD.is_deleted) THEN
        NEW.deleted_at := clock_timestamp();
        NEW.deleted_by := nullif(current_setting('budget.actor_employee_id', true), '')::uuid;
    ELSIF TG_OP = 'UPDATE' THEN
        NEW.deleted_at := OLD.deleted_at;
        NEW.deleted_by := OLD.deleted_by;
    END IF;

    -- DB-01: WORK and UNFORESEEN only in a WORKS proposal; HEAD only in the other three.
    IF (NEW.line_type IN ('WORK', 'UNFORESEEN')) <> (p.category = 'WORKS') THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001',
            DETAIL = format('a %s line is not accepted in a %s proposal', NEW.line_type, p.category);
    END IF;

    -- DB-03: a code marked not for use is refused as a new reference.
    IF NEW.account_code_id IS NOT NULL
       AND (TG_OP = 'INSERT' OR NEW.account_code_id IS DISTINCT FROM OLD.account_code_id)
       AND EXISTS (SELECT 1 FROM budget.budget_head_master h
                   WHERE h.account_code_id = NEW.account_code_id AND h.not_for_use) THEN
        RAISE EXCEPTION 'M-05' USING ERRCODE = 'P0001';
    END IF;

    -- DB-04: the work of a WORK line belongs to the proposal's Division.
    IF NEW.work_id IS NOT NULL AND NOT EXISTS (
        SELECT 1 FROM budget.work w WHERE w.id = NEW.work_id AND w.division_id = p.origin_division_id
    ) THEN
        RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'the work is not of the proposal''s Division';
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- budget.fn_capture_figure_version(): DB-14, DDD §6.3. AFTER INSERT OR UPDATE OF rbe_amount, obe_next_amount
-- on T-30, SECURITY DEFINER: writes one T-31 row per figure inserted or changed, with the holder at the time,
-- the state, and the justification / continue-without-justification / role / on-behalf-of from the actor
-- context (DDD §7.1, BUD-122, BUD-127). T-31's [C] columns are filled by fn_require_actor on this insert,
-- and fn_audit_row writes its T-51 row.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_capture_figure_version() RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    p        record;
    v_source text;
    v_action text := coalesce(current_setting('budget.action_code', true), '');
BEGIN
    SELECT pr.holder_office_id, pr.origin_office_id, pr.state_code INTO p
    FROM budget.proposal pr WHERE pr.proposal_id = NEW.proposal_id;

    v_source := CASE
        WHEN v_action LIKE 'UPLOAD%' OR v_action LIKE 'BULK_UPLOAD%' THEN 'UPLOAD'
        WHEN p.holder_office_id = p.origin_office_id THEN 'ENTRY'
        ELSE 'ALTERATION'
    END;

    IF TG_OP = 'INSERT' OR NEW.rbe_amount IS DISTINCT FROM OLD.rbe_amount THEN
        INSERT INTO budget.figure_version (demand_line_id, figure, value, level_office_id, state_code, change_source,
                                           justification, continued_without_justification, role_code, on_behalf_of_post)
        VALUES (NEW.demand_line_id, 'RBE', NEW.rbe_amount, p.holder_office_id, p.state_code, v_source,
                nullif(current_setting('budget.justification', true), ''),
                coalesce(current_setting('budget.no_justification', true), '') = 'true',
                current_setting('budget.actor_role_code', true),
                nullif(current_setting('budget.on_behalf_of_post', true), ''));
    END IF;
    IF TG_OP = 'INSERT' OR NEW.obe_next_amount IS DISTINCT FROM OLD.obe_next_amount THEN
        INSERT INTO budget.figure_version (demand_line_id, figure, value, level_office_id, state_code, change_source,
                                           justification, continued_without_justification, role_code, on_behalf_of_post)
        VALUES (NEW.demand_line_id, 'OBE_NEXT', NEW.obe_next_amount, p.holder_office_id, p.state_code, v_source,
                nullif(current_setting('budget.justification', true), ''),
                coalesce(current_setting('budget.no_justification', true), '') = 'true',
                current_setting('budget.actor_role_code', true),
                nullif(current_setting('budget.on_behalf_of_post', true), ''));
    END IF;
    RETURN NULL;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-32 protect: DB-15 with the §5.5 row d exception (CR-DDD-04). An observation is fixed once written,
-- save that (a) its reply may be written once, replied_at/by then being set from the actor context, and
-- (b) inside a RETURN action selected_for_return may go false -> true and returned_by_movement_id
-- null -> value, once each. On INSERT, DB-04: the line is of the observation's proposal.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_observation_protect() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    c_open   constant text[] := ARRAY['reply_text', 'replied_at', 'replied_by', 'selected_for_return', 'returned_by_movement_id'];
    v_return boolean := coalesce(current_setting('budget.action_code', true), '') = 'RETURN';
BEGIN
    IF TG_OP = 'INSERT' THEN
        IF NEW.demand_line_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM budget.demand_line l WHERE l.demand_line_id = NEW.demand_line_id AND l.proposal_id = NEW.proposal_id
        ) THEN
            RAISE EXCEPTION 'DB-04' USING ERRCODE = 'P0001', DETAIL = 'the observation''s line is of another proposal';
        END IF;
        IF NEW.reply_text IS NOT NULL OR NEW.returned_by_movement_id IS NOT NULL THEN
            RAISE EXCEPTION 'DB-15' USING ERRCODE = 'P0001', DETAIL = 'an observation is written without reply or return';
        END IF;
        NEW.replied_at := NULL;
        NEW.replied_by := NULL;
        RETURN NEW;
    END IF;

    IF (to_jsonb(NEW) - c_open) IS DISTINCT FROM (to_jsonb(OLD) - c_open) THEN
        RAISE EXCEPTION 'DB-15' USING ERRCODE = 'P0001', DETAIL = 'an observation is fixed once written';
    END IF;

    -- (a) the reply, written once.
    IF NEW.reply_text IS DISTINCT FROM OLD.reply_text THEN
        IF OLD.reply_text IS NOT NULL OR btrim(coalesce(NEW.reply_text, '')) = '' THEN
            RAISE EXCEPTION 'DB-15' USING ERRCODE = 'P0001', DETAIL = 'the reply may be written once';
        END IF;
        NEW.replied_at := clock_timestamp();
        NEW.replied_by := nullif(current_setting('budget.actor_employee_id', true), '')::uuid;
    ELSE
        NEW.replied_at := OLD.replied_at;
        NEW.replied_by := OLD.replied_by;
    END IF;

    -- (b) the two return columns, once each, inside a RETURN.
    IF NEW.selected_for_return IS DISTINCT FROM OLD.selected_for_return
       AND (OLD.selected_for_return OR NOT v_return) THEN
        RAISE EXCEPTION 'DB-15' USING ERRCODE = 'P0001', DETAIL = 'selected_for_return is set once, by a RETURN';
    END IF;
    IF NEW.returned_by_movement_id IS DISTINCT FROM OLD.returned_by_movement_id
       AND (OLD.returned_by_movement_id IS NOT NULL OR NOT v_return) THEN
        RAISE EXCEPTION 'DB-15' USING ERRCODE = 'P0001', DETAIL = 'returned_by_movement_id is set once, by a RETURN';
    END IF;
    RETURN NEW;
END
$fn$;
