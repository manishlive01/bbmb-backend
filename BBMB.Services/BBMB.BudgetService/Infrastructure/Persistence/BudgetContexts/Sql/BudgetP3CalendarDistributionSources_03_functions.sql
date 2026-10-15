-- BudgetP3CalendarDistributionSources step 3: DB-18 (DDD §6.1) and the DB-01 placement of a distribution.
-- Where a rule has an SRS message the exception text is that number so the global handler shows the catalogue
-- text (design §4.9): the distribution ceiling raises M-14 (VR-07).

-- ---------------------------------------------------------------------------------------------------
-- T-39 BEFORE INSERT OR UPDATE: DB-01. A distribution office is a Circle or Division within the charge of
-- the recommendation's CE (BUD-156); recommendation and office are fixed once written.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_distribution_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_ce_id bigint;
BEGIN
    IF TG_OP = 'UPDATE' AND (NEW.recommendation_id <> OLD.recommendation_id OR NEW.office_id <> OLD.office_id) THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'recommendation and office of a distribution are fixed';
    END IF;
    SELECT o.ce_id INTO v_ce_id
    FROM budget.ce_recommendation r JOIN budget.office o ON o.office_id = r.ce_office_id
    WHERE r.recommendation_id = NEW.recommendation_id;
    -- The office must be a Circle or Division whose CE charge is the recommendation's CE office.
    IF NOT EXISTS (
        SELECT 1 FROM budget.office d
        WHERE d.office_id = NEW.office_id AND d.office_level IN ('DIVISION', 'CIRCLE') AND d.ce_id = v_ce_id
    ) THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001',
            DETAIL = 'a distribution is to a Circle or Division within the charge of the recommendation''s CE (DB-01, BUD-156)';
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- DB-18, VR-07 (M-14), at commit. The distribution of each recommendation totals, column by column, no more
-- than the figure recommended. A CONSTRAINT TRIGGER DEFERRABLE INITIALLY DEFERRED so the whole upsert of a
-- recommendation's shares is judged as a set (DDD §6.1 "checked as a set and not row by row"), not row by row.
-- A null recommended figure is treated as nil for its column (nothing may be distributed from an unset figure).
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_distribution_ceiling_check() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_rbe_recommended budget.rupees;
    v_obe_recommended budget.rupees;
    v_rbe_total budget.rupees;
    v_obe_total budget.rupees;
BEGIN
    SELECT coalesce(r.rbe_recommended, 0), coalesce(r.obe_next_recommended, 0)
    INTO v_rbe_recommended, v_obe_recommended
    FROM budget.ce_recommendation r WHERE r.recommendation_id = NEW.recommendation_id;

    SELECT coalesce(sum(d.rbe_amount), 0), coalesce(sum(d.obe_next_amount), 0)
    INTO v_rbe_total, v_obe_total
    FROM budget.distribution d WHERE d.recommendation_id = NEW.recommendation_id;

    IF v_rbe_total > v_rbe_recommended OR v_obe_total > v_obe_recommended THEN
        RAISE EXCEPTION 'M-14' USING ERRCODE = 'P0001',
            DETAIL = format('distribution of recommendation %s exceeds the recommended figure (RBE %s/%s, OBE %s/%s)',
                            NEW.recommendation_id, v_rbe_total, v_rbe_recommended, v_obe_total, v_obe_recommended);
    END IF;
    RETURN NULL;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-37 AFTER UPDATE of the recommended figures: BUD-279. A change that leaves the existing distribution of
-- the recommendation above the new figure in either column sets redistribution_required; it is cleared only
-- on the CE's confirmation with VR-07 satisfied (DB-18, BUD-280), which the Confirm slice does. SECURITY
-- DEFINER so the flag is set regardless of the actor's table privilege.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_recommendation_redistribution_flag() RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    v_rbe_total budget.rupees;
    v_obe_total budget.rupees;
BEGIN
    IF NEW.rbe_recommended IS DISTINCT FROM OLD.rbe_recommended
       OR NEW.obe_next_recommended IS DISTINCT FROM OLD.obe_next_recommended THEN
        SELECT coalesce(sum(d.rbe_amount), 0), coalesce(sum(d.obe_next_amount), 0)
        INTO v_rbe_total, v_obe_total
        FROM budget.distribution d WHERE d.recommendation_id = NEW.recommendation_id;
        IF v_rbe_total > coalesce(NEW.rbe_recommended, 0) OR v_obe_total > coalesce(NEW.obe_next_recommended, 0) THEN
            UPDATE budget.ce_recommendation SET redistribution_required = true
            WHERE recommendation_id = NEW.recommendation_id AND NOT redistribution_required;
        END IF;
    END IF;
    RETURN NULL;
END
$fn$;
