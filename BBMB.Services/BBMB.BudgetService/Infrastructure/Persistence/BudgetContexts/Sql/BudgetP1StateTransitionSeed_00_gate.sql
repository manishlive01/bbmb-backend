-- BudgetP1StateTransitionSeed step 0, design §11.1 rule 9 and §11.4: the only migration under gate
-- p1_state_model. T-16 differs from SRS §5 in the rows marked in design §6.2 (§5.5 rows j and l); it runs in a
-- controlled environment only after BBMB-FIN records the decision (OI-19, OI-20, OI-21, OI-22, OI-42).
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.p1_state_model', true), '') = '' THEN
        RAISE EXCEPTION 'gate p1_state_model not set: decision CR-SRS-01 / CR-DDD-08 / CR-DDD-10 not recorded';
    END IF;
END
$$;
