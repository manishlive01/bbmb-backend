-- BudgetP1ProposalSpine step 0, design §11.1 rule 9, §11.4: gated on p1_ddd_deviations (the T-32 protect
-- exception of §5.5 row d is in this migration). The gate guard is the first statement.
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.p1_ddd_deviations', true), '') = '' THEN
        RAISE EXCEPTION 'gate p1_ddd_deviations not set: decision CR-DDD-01 / CR-DDD-04 not recorded';
    END IF;
END
$$;
