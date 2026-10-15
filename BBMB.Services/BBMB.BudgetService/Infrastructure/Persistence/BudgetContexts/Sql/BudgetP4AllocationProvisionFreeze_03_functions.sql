-- BudgetP4AllocationProvisionFreeze step 3: the DDD §6 database rules for allocation, provision, freeze,
-- estimate allocation and unforeseen work. Where a rule has an SRS message, the exception text is that number
-- so the global handler shows the catalogue text (design §4.9): DB-23 raises M-28, DB-26 raises M-29, the
-- freeze precondition raises M-30. All functions pin search_path; the balance-at-commit checks are
-- CONSTRAINT TRIGGERs DEFERRABLE INITIALLY DEFERRED so a multi-row allocation or authorisation is judged as a
-- set, not row by row.

-- ===================================================================================================
-- T-41 provision BEFORE UPDATE: DB-05. A provision is written once; the only column that may change is
-- superseded_by_provision_id, and it may be set once (null -> value), never cleared or re-pointed (BUD-285).
-- ===================================================================================================
CREATE FUNCTION budget.fn_provision_write_once() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NEW.fin_year IS DISTINCT FROM OLD.fin_year
       OR NEW.basis IS DISTINCT FROM OLD.basis
       OR NEW.category IS DISTINCT FROM OLD.category
       OR NEW.holder_type IS DISTINCT FROM OLD.holder_type
       OR NEW.work_id IS DISTINCT FROM OLD.work_id
       OR NEW.office_id IS DISTINCT FROM OLD.office_id
       OR NEW.account_code_id IS DISTINCT FROM OLD.account_code_id
       OR NEW.demand_line_id IS DISTINCT FROM OLD.demand_line_id
       OR NEW.recommendation_id IS DISTINCT FROM OLD.recommendation_id
       OR NEW.distribution_id IS DISTINCT FROM OLD.distribution_id
       OR NEW.allocation_id IS DISTINCT FROM OLD.allocation_id
       OR NEW.effective_from IS DISTINCT FROM OLD.effective_from
       OR NEW.budgetary_check IS DISTINCT FROM OLD.budgetary_check THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001',
            DETAIL = 'a provision is written once; only its supersession marker may change';
    END IF;
    IF OLD.superseded_by_provision_id IS NOT NULL
       AND NEW.superseded_by_provision_id IS DISTINCT FROM OLD.superseded_by_provision_id THEN
        RAISE EXCEPTION 'DB-05' USING ERRCODE = 'P0001',
            DETAIL = 'the supersession marker of a provision is set once (BUD-285)';
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-42 provision_movement BEFORE INSERT: DB-19 and DB-22, judged when the movement is written.
--  - DB-19: an ALLOCATION movement is refused for a cycle that is frozen. The cycle is that of the
--    movement's allocation (ALLOCATION movements always carry allocation_id, per the CHECK).
--  - DB-22: no movement of any type is accepted against a provision whose financial year is closed in
--    master.fa_financial_years (BUD-251).
-- A provision_movement is append-only (fn_forbid_change on UPDATE/DELETE), so the rule is on INSERT only.
-- ===================================================================================================
CREATE FUNCTION budget.fn_provision_movement_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_fin_year int;
    v_cycle_id bigint;
BEGIN
    -- DB-22: the provision's year must be open.
    SELECT p.fin_year INTO v_fin_year FROM budget.provision p WHERE p.provision_id = NEW.provision_id;
    IF EXISTS (SELECT 1 FROM budget.fa_financial_years y
               WHERE y.fin_year_closing = v_fin_year AND y.closed IS NOT NULL) THEN
        RAISE EXCEPTION 'DB-22' USING ERRCODE = 'P0001',
            DETAIL = format('no movement is accepted against a provision of the closed year %s (BUD-251)', v_fin_year);
    END IF;

    -- DB-19: an ALLOCATION movement is refused once the cycle is frozen.
    IF NEW.movement_type = 'ALLOCATION' THEN
        SELECT a.cycle_id INTO v_cycle_id FROM budget.allocation a WHERE a.allocation_id = NEW.allocation_id;
        IF EXISTS (SELECT 1 FROM budget.freeze f WHERE f.cycle_id = v_cycle_id) THEN
            RAISE EXCEPTION 'DB-19' USING ERRCODE = 'P0001',
                DETAIL = format('cycle %s is frozen; no allocation movement is accepted', v_cycle_id);
        END IF;
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-42 provision_movement AT COMMIT (CONSTRAINT TRIGGER, DEFERRABLE INITIALLY DEFERRED): DB-21. The movements
-- of one re-appropriation, or of one unforeseen authorisation, sum to nil, so provision is neither created
-- nor lost (BUD-204, BUD-298). Grouped by the reference that ties the set together. ALLOCATION movements are
-- not balanced this way (they create provision from the budget) and are excluded. reappropriation_id is the
-- P5 column; until P5 writes it, no REAPPROP movement exists, so the reappropriation branch is inert.
-- ===================================================================================================
CREATE FUNCTION budget.fn_provision_movement_balance() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_sum budget.rupees;
BEGIN
    IF NEW.unforeseen_authorisation_id IS NOT NULL THEN
        SELECT coalesce(sum(m.amount), 0) INTO v_sum
        FROM budget.provision_movement m
        WHERE m.unforeseen_authorisation_id = NEW.unforeseen_authorisation_id;
        IF v_sum <> 0 THEN
            RAISE EXCEPTION 'DB-21' USING ERRCODE = 'P0001',
                DETAIL = format('the movements of unforeseen authorisation %s do not balance (sum %s)',
                                NEW.unforeseen_authorisation_id, v_sum);
        END IF;
    ELSIF NEW.reappropriation_id IS NOT NULL THEN
        SELECT coalesce(sum(m.amount), 0) INTO v_sum
        FROM budget.provision_movement m
        WHERE m.reappropriation_id = NEW.reappropriation_id;
        IF v_sum <> 0 THEN
            RAISE EXCEPTION 'DB-21' USING ERRCODE = 'P0001',
                DETAIL = format('the movements of re-appropriation %s do not balance (sum %s)',
                                NEW.reappropriation_id, v_sum);
        END IF;
    END IF;
    RETURN NULL;
END
$fn$;

-- ===================================================================================================
-- T-43 estimate_allocation AT COMMIT (CONSTRAINT TRIGGER, DEFERRABLE INITIALLY DEFERRED): DB-23, VR-12, M-28.
-- The total allocated across the estimates of one provision is not above the amount available on that
-- provision (V-02). Judged as a set so a multi-line save is weighed once (BUD-290, BUD-291).
-- ===================================================================================================
CREATE FUNCTION budget.fn_estimate_allocation_ceiling() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_total budget.rupees;
    v_available budget.rupees;
BEGIN
    SELECT coalesce(sum(ea.amount), 0) INTO v_total
    FROM budget.estimate_allocation ea WHERE ea.provision_id = NEW.provision_id;

    SELECT coalesce(vb.available, 0) INTO v_available
    FROM budget.v_provision_balance vb WHERE vb.provision_id = NEW.provision_id;

    IF v_total > v_available THEN
        RAISE EXCEPTION 'M-28' USING ERRCODE = 'P0001',
            DETAIL = format('estimate allocation of provision %s (%s) exceeds the amount available (%s)',
                            NEW.provision_id, v_total, v_available);
    END IF;
    RETURN NULL;
END
$fn$;

-- ===================================================================================================
-- T-48 unforeseen_request BEFORE INSERT OR UPDATE: DB-25 (BUD-294). The request is for a work carrying a
-- recorded administrative approval (a T-25 row) and no work code. The one-request-per-work UK is an index;
-- this rule guards the approval and no-code preconditions.
-- ===================================================================================================
CREATE FUNCTION budget.fn_unforeseen_request_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM budget.admin_approval a WHERE a.work_id = NEW.work_id) THEN
        RAISE EXCEPTION 'DB-25' USING ERRCODE = 'P0001',
            DETAIL = 'an unforeseen request is for a work carrying a recorded administrative approval (BUD-294)';
    END IF;
    IF EXISTS (SELECT 1 FROM budget.work w WHERE w.id = NEW.work_id AND w.work_code IS NOT NULL) THEN
        RAISE EXCEPTION 'DB-25' USING ERRCODE = 'P0001',
            DETAIL = 'an unforeseen request is for a work with no work code (BUD-294)';
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-50 unforeseen_authorisation BEFORE INSERT: DB-26, VR-13, M-29. Neither the current-year nor the next-year
-- amount is above the balance available on the provision drawn upon (V-02). The provisions drawn upon are
-- those named on the request standing AT_CE.
-- ===================================================================================================
CREATE FUNCTION budget.fn_unforeseen_authorisation_balance() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_current_provision bigint;
    v_next_provision bigint;
    v_available budget.rupees;
BEGIN
    SELECT r.current_provision_id, r.next_provision_id
    INTO v_current_provision, v_next_provision
    FROM budget.unforeseen_request r WHERE r.unforeseen_request_id = NEW.unforeseen_request_id;

    IF NEW.amount_current IS NOT NULL THEN
        SELECT coalesce(vb.available, 0) INTO v_available
        FROM budget.v_provision_balance vb WHERE vb.provision_id = v_current_provision;
        IF NEW.amount_current > v_available THEN
            RAISE EXCEPTION 'M-29' USING ERRCODE = 'P0001',
                DETAIL = format('the current-year amount (%s) exceeds the unforeseen balance available (%s)',
                                NEW.amount_current, v_available);
        END IF;
    END IF;

    IF NEW.amount_next IS NOT NULL THEN
        SELECT coalesce(vb.available, 0) INTO v_available
        FROM budget.v_provision_balance vb WHERE vb.provision_id = v_next_provision;
        IF NEW.amount_next > v_available THEN
            RAISE EXCEPTION 'M-29' USING ERRCODE = 'P0001',
                DETAIL = format('the next-year amount (%s) exceeds the unforeseen balance available (%s)',
                                NEW.amount_next, v_available);
        END IF;
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-37 ce_recommendation BEFORE UPDATE: DB-17 (BUD-301). Once an allocation of the recommendation's cycle
-- exists, a change to a recommended figure must cite the minutes allocation it gives effect to; the citation
-- is minutes_allocation_id, and it must name an allocation of this recommendation's cycle.
-- ===================================================================================================
CREATE FUNCTION budget.fn_recommendation_cites_minutes() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF (NEW.rbe_recommended IS DISTINCT FROM OLD.rbe_recommended
        OR NEW.obe_next_recommended IS DISTINCT FROM OLD.obe_next_recommended)
       AND EXISTS (SELECT 1 FROM budget.allocation a WHERE a.cycle_id = NEW.cycle_id) THEN
        IF NEW.minutes_allocation_id IS NULL
           OR NEW.minutes_allocation_id IS NOT DISTINCT FROM OLD.minutes_allocation_id THEN
            RAISE EXCEPTION 'DB-17' USING ERRCODE = 'P0001',
                DETAIL = 'a recommended figure changed after an allocation must cite the minutes allocation (BUD-301)';
        END IF;
        IF NOT EXISTS (SELECT 1 FROM budget.allocation a
                       WHERE a.allocation_id = NEW.minutes_allocation_id AND a.cycle_id = NEW.cycle_id) THEN
            RAISE EXCEPTION 'DB-17' USING ERRCODE = 'P0001',
                DETAIL = 'the cited minutes allocation is not an allocation of this recommendation''s cycle';
        END IF;
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-44 freeze BEFORE INSERT: DB-19, M-30 (BUD-190, BUD-193). The freeze of a cycle is refused unless the
-- FULL_BOARD allocation of the cycle exists, no T-37 recommendation of the cycle carries
-- redistribution_required, and every proposal of the cycle stands at S-15. On success, AFTER INSERT, the
-- cycle's status becomes FROZEN. The freeze is append-only, so there is no unfreeze (BUD-193).
-- ===================================================================================================
CREATE FUNCTION budget.fn_freeze_preconditions() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM budget.allocation a
                   WHERE a.cycle_id = NEW.cycle_id AND a.body = 'FULL_BOARD') THEN
        RAISE EXCEPTION 'M-19' USING ERRCODE = 'P0001',
            DETAIL = format('cycle %s has no FULL_BOARD allocation; it cannot be frozen (BUD-190)', NEW.cycle_id);
    END IF;
    IF EXISTS (SELECT 1 FROM budget.ce_recommendation r
               WHERE r.cycle_id = NEW.cycle_id AND r.redistribution_required) THEN
        RAISE EXCEPTION 'M-30' USING ERRCODE = 'P0001',
            DETAIL = 'a recommendation of the cycle still requires redistribution';
    END IF;
    IF EXISTS (SELECT 1 FROM budget.proposal p
               WHERE p.cycle_id = NEW.cycle_id AND p.state_code <> 'S-15') THEN
        RAISE EXCEPTION 'M-19' USING ERRCODE = 'P0001',
            DETAIL = format('cycle %s has a proposal not at S-15; it cannot be frozen (BUD-193)', NEW.cycle_id);
    END IF;
    RETURN NEW;
END
$fn$;

CREATE FUNCTION budget.fn_freeze_set_cycle_frozen() RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    UPDATE budget.budget_cycle SET status = 'FROZEN'
    WHERE cycle_id = NEW.cycle_id AND status <> 'FROZEN';
    RETURN NULL;
END
$fn$;
