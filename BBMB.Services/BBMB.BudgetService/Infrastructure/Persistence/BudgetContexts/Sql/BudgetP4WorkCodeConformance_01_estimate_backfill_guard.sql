-- BudgetP4WorkCodeConformance step 1 (design §4.6 step 1): backfill estimate.work_id from the old
-- work_code match, then guard. estimate carries no actor or audit trigger (it is a legacy table outside
-- T-01..T-51), so this step needs no migration actor. The AddColumn was hand-edited to nullable with no
-- default (§11.1 rule 2); the column is NULL here so an unmatched estimate is caught by the guard rather
-- than silently defaulting to work_id = 0 and being dropped by the step-6 inner join.
UPDATE budget.estimate e
SET work_id = w.id
FROM budget.work w
WHERE w.work_code = e.work_code;

-- Guard: every estimate must resolve to exactly one work by its old integer code. If any does not, abort
-- the whole migration and list the orphans. This runs BEFORE SET NOT NULL and before any uniqueness drop.
DO $$
DECLARE
    v_orphans text;
    v_count   bigint;
BEGIN
    SELECT count(*), string_agg(format('(work_code=%s, estimate_code=%s)', e.work_code, e.estimate_code), ', ')
    INTO v_count, v_orphans
    FROM budget.estimate e
    WHERE e.work_id IS NULL;

    IF v_count > 0 THEN
        RAISE EXCEPTION 'estimate.work_id backfill left % unmatched estimate row(s): %', v_count, v_orphans;
    END IF;
END
$$;
