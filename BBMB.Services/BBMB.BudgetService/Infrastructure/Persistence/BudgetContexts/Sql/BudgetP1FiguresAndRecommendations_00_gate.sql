-- BudgetP1FiguresAndRecommendations step 0 (plan decision PD-1): gated with the rest of BudgetP1ProposalSpine
-- on p1_ddd_deviations (design §11.1 rule 9). The gate guard is the first statement.
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.p1_ddd_deviations', true), '') = '' THEN
        RAISE EXCEPTION 'gate p1_ddd_deviations not set: decision CR-DDD-01 / CR-DDD-04 not recorded';
    END IF;
END
$$;
