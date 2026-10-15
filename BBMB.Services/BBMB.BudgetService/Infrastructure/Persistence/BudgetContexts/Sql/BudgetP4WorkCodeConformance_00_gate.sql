-- BudgetP4WorkCodeConformance step 0 (design §4.6 step 0, §11.1 rule 9): the work-code reset is a
-- DESTRUCTIVE data migration (int work_code -> varchar(12), every existing code moved to work_code_prev and
-- the live code nulled). It must not run in a controlled environment until BBMB has recorded a disposition
-- for existing work codes (OI-11) and set the gate to that decision reference. The gate value becomes
-- budget.justification for the migration actor, so every T-51 row of the reset carries the authority for it.
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.work_code_reset', true), '') = '' THEN
        RAISE EXCEPTION 'gate work_code_reset not set: decision OI-11 (disposition of existing work codes) not recorded';
    END IF;
END
$$;

-- Pre-migration count of works, captured before any shaping, for the step-6 exit guard. A TEMP table is
-- dropped automatically at the end of the session; it carries no audit and references no actor.
CREATE TEMP TABLE p4_pre AS SELECT count(*)::bigint AS works FROM budget.work;
