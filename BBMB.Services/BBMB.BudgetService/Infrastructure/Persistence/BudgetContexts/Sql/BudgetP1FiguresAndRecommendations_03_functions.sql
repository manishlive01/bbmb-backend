-- BudgetP1FiguresAndRecommendations step 3: DB-16, DB-01 (T-37) and DB-14 (T-38) (DDD §6.1, §6.3).

-- ---------------------------------------------------------------------------------------------------
-- T-36 on INSERT OR UPDATE: DB-16. An opening figure is accepted only in the first cycle and is fixed once
-- the origin proposal is first submitted (BUD-306). The origin proposal of a WORK subject is the WORKS
-- proposal of the work's Division office; of a HEAD subject, a head-wise proposal of that office.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_opening_figure_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
DECLARE
    r record;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM budget.budget_cycle c WHERE c.cycle_id = NEW.cycle_id AND c.is_first_cycle) THEN
        RAISE EXCEPTION 'DB-16' USING ERRCODE = 'P0001', DETAIL = 'opening figures are accepted only in the first cycle';
    END IF;

    FOR r IN SELECT NEW.cycle_id AS cycle_id, NEW.subject_type AS subject_type, NEW.work_id AS work_id, NEW.office_id AS office_id
             UNION ALL
             SELECT OLD.cycle_id, OLD.subject_type, OLD.work_id, OLD.office_id WHERE TG_OP = 'UPDATE'
    LOOP
        IF EXISTS (
            SELECT 1
            FROM budget.proposal p
            JOIN budget.office o ON o.office_id = p.origin_office_id
            LEFT JOIN budget.work w ON w.id = r.work_id
            WHERE p.cycle_id = r.cycle_id
              AND p.first_submitted_at IS NOT NULL
              AND ((r.subject_type = 'WORK' AND p.category = 'WORKS' AND o.division_id = w.division_id)
                OR (r.subject_type = 'HEAD' AND p.category <> 'WORKS' AND p.origin_office_id = r.office_id))
        ) THEN
            RAISE EXCEPTION 'DB-16' USING ERRCODE = 'P0001',
                DETAIL = 'an opening figure is fixed once the origin proposal is first submitted';
        END IF;
    END LOOP;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- T-37 on INSERT OR UPDATE: DB-01. A recommendation is at a CE office (BUD-303); the identifying columns
-- are fixed.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_recommendation_rules() RETURNS trigger
    LANGUAGE plpgsql
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM budget.office o WHERE o.office_id = NEW.ce_office_id AND o.office_level = 'CE') THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'a recommendation is made for a CE office';
    END IF;
    IF TG_OP = 'UPDATE' AND (NEW.cycle_id <> OLD.cycle_id OR NEW.category <> OLD.category
                             OR NEW.ce_office_id <> OLD.ce_office_id OR NEW.account_code_id <> OLD.account_code_id) THEN
        RAISE EXCEPTION 'DB-01' USING ERRCODE = 'P0001', DETAIL = 'cycle, category, CE and code of a recommendation are fixed';
    END IF;
    RETURN NEW;
END
$fn$;

-- ---------------------------------------------------------------------------------------------------
-- budget.fn_capture_recommendation_version(): DB-14, DDD §6.3. AFTER INSERT OR UPDATE on T-37, SECURITY
-- DEFINER: every insertion or change of the recommended figures or the concurrence writes the next T-38
-- version (numbered from 1 without gap). T-38's [C] columns are filled by fn_require_actor and its T-51 row
-- by fn_audit_row.
-- ---------------------------------------------------------------------------------------------------
CREATE FUNCTION budget.fn_capture_recommendation_version() RETURNS trigger
    LANGUAGE plpgsql
    SECURITY DEFINER
    SET search_path = budget, pg_temp
AS $fn$
BEGIN
    IF TG_OP = 'INSERT'
       OR NEW.rbe_recommended IS DISTINCT FROM OLD.rbe_recommended
       OR NEW.obe_next_recommended IS DISTINCT FROM OLD.obe_next_recommended
       OR NEW.concurrence_ref IS DISTINCT FROM OLD.concurrence_ref THEN
        INSERT INTO budget.recommendation_version (recommendation_id, version_no, rbe_recommended, obe_next_recommended, concurrence_ref)
        SELECT NEW.recommendation_id, coalesce(max(v.version_no), 0) + 1, NEW.rbe_recommended, NEW.obe_next_recommended,
               NEW.concurrence_ref
        FROM budget.recommendation_version v
        WHERE v.recommendation_id = NEW.recommendation_id;
    END IF;
    RETURN NULL;
END
$fn$;
