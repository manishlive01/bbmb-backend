-- BudgetP5Reappropriation step 3: DB-24 (DDD §5.8). The re-appropriation status model and the surrender rules.
-- Where a rule has an SRS message the exception text is that number (design §4.9): the surrender ceiling raises
-- M-18 (VR-08).

-- ===================================================================================================
-- T-46 reappropriation BEFORE UPDATE: DB-24. The status moves from OPEN once (OPEN -> SUBMITTED or OPEN ->
-- ABANDONED), never back and never between SUBMITTED and ABANDONED. The destination of an entry is fixed once
-- OPEN is left.
-- ===================================================================================================
CREATE FUNCTION budget.fn_reappropriation_status_once() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF OLD.status <> 'OPEN' AND NEW.status <> OLD.status THEN
        RAISE EXCEPTION 'DB-24' USING ERRCODE = 'P0001',
            DETAIL = format('a re-appropriation moves from OPEN only, once (was %s)', OLD.status);
    END IF;
    IF OLD.status <> 'OPEN' AND (
           NEW.destination_provision_id IS DISTINCT FROM OLD.destination_provision_id
        OR NEW.destination_new_work_id IS DISTINCT FROM OLD.destination_new_work_id
        OR NEW.additional_amount IS DISTINCT FROM OLD.additional_amount) THEN
        RAISE EXCEPTION 'DB-24' USING ERRCODE = 'P0001',
            DETAIL = 'the destination and amount of a re-appropriation are fixed once it leaves OPEN';
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-47 reappropriation_source BEFORE INSERT OR UPDATE: DB-24 and §5.5 row c. A source may be written or changed
-- only while its entry is OPEN; it may not be the entry's destination provision; is_removed is one-way
-- false -> true.
-- ===================================================================================================
CREATE FUNCTION budget.fn_reappropriation_source_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_status varchar(9);
    v_destination_provision bigint;
BEGIN
    SELECT status, destination_provision_id INTO v_status, v_destination_provision
    FROM budget.reappropriation WHERE reappropriation_id = NEW.reappropriation_id;

    IF v_status <> 'OPEN' THEN
        RAISE EXCEPTION 'DB-24' USING ERRCODE = 'P0001',
            DETAIL = 'a surrender line may be added or changed only while the entry is OPEN';
    END IF;
    IF v_destination_provision IS NOT NULL AND NEW.provision_id = v_destination_provision THEN
        RAISE EXCEPTION 'DB-24' USING ERRCODE = 'P0001',
            DETAIL = 'a surrender source may not be the entry''s destination provision';
    END IF;
    IF TG_OP = 'UPDATE' AND OLD.is_removed AND NOT NEW.is_removed THEN
        RAISE EXCEPTION 'DB-24' USING ERRCODE = 'P0001',
            DETAIL = 'is_removed is one-way (a removed surrender line cannot be restored)';
    END IF;
    RETURN NEW;
END
$fn$;

-- ===================================================================================================
-- T-47 AT COMMIT (CONSTRAINT TRIGGER, DEFERRABLE INITIALLY DEFERRED): DB-24, VR-08 (M-18). The cumulative
-- amount of the non-removed sources of an entry is not above the entry's additional amount; on a SUBMITTED
-- entry it must equal it. Judged as a set so a multi-line save is weighed once (BUD-199, BUD-203).
-- ===================================================================================================
CREATE FUNCTION budget.fn_reappropriation_balance() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_additional budget.rupees;
    v_status varchar(9);
    v_total budget.rupees;
BEGIN
    SELECT additional_amount, status INTO v_additional, v_status
    FROM budget.reappropriation WHERE reappropriation_id = NEW.reappropriation_id;
    IF v_additional IS NULL THEN
        RETURN NULL; -- the entry was removed in the same transaction; nothing to check
    END IF;

    SELECT coalesce(sum(amount), 0) INTO v_total
    FROM budget.reappropriation_source
    WHERE reappropriation_id = NEW.reappropriation_id AND NOT is_removed;

    IF v_total > v_additional THEN
        RAISE EXCEPTION 'M-18' USING ERRCODE = 'P0001',
            DETAIL = format('the surrender total (%s) exceeds the additional amount (%s)', v_total, v_additional);
    END IF;
    IF v_status = 'SUBMITTED' AND v_total <> v_additional THEN
        RAISE EXCEPTION 'M-18' USING ERRCODE = 'P0001',
            DETAIL = format('on submission the surrender total (%s) must equal the additional amount (%s)', v_total, v_additional);
    END IF;
    RETURN NULL;
END
$fn$;

-- ===================================================================================================
-- T-46 AT COMMIT (CONSTRAINT TRIGGER, DEFERRABLE INITIALLY DEFERRED): VR-08 (M-18) on submission. When an entry
-- becomes SUBMITTED, its non-removed surrenders must equal the additional amount (BUD-203). This complements
-- fn_reappropriation_balance, which fires on source changes, for a status flip that touches no source row.
-- ===================================================================================================
CREATE FUNCTION budget.fn_reappropriation_submit_balance() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_total budget.rupees;
BEGIN
    IF NEW.status <> 'SUBMITTED' THEN
        RETURN NULL;
    END IF;
    SELECT coalesce(sum(amount), 0) INTO v_total
    FROM budget.reappropriation_source
    WHERE reappropriation_id = NEW.reappropriation_id AND NOT is_removed;
    IF v_total <> NEW.additional_amount THEN
        RAISE EXCEPTION 'M-18' USING ERRCODE = 'P0001',
            DETAIL = format('on submission the surrender total (%s) must equal the additional amount (%s)', v_total, NEW.additional_amount);
    END IF;
    RETURN NULL;
END
$fn$;
