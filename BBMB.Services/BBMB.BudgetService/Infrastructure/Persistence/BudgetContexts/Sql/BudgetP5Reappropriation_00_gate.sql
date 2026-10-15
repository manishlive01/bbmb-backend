-- BudgetP5Reappropriation step 0: gate guard (design §11.1 rule 9, §5.5 row c). This migration carries the
-- DDD deviation CR-DDD-03 (T-47.is_removed, a column DB-13 forbids by its "no delete" rule), so it does not
-- run in a controlled environment until BBMB-FIN's decision is recorded and its reference is set in
-- bbmb_gate.p5_ddd_deviations. In development and the Testcontainers tests the gate holds 'TEST'.
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.p5_ddd_deviations', true), '') = '' THEN
        RAISE EXCEPTION 'gate p5_ddd_deviations not set: decision CR-DDD-03 not recorded';
    END IF;
END
$$;
