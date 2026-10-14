-- BudgetP1ProposalSpineMasters step 0, design §11.1 rule 9 and plan decision PD-1: this migration carries
-- the DDD deviations of §5.5 row a (work.pre_ddd, nullable creation columns on work) and is gated with the
-- rest of BudgetP1ProposalSpine. The DBA sets the gate only once the decision is recorded.
DO $$
BEGIN
    IF coalesce(current_setting('bbmb_gate.p1_ddd_deviations', true), '') = '' THEN
        RAISE EXCEPTION 'gate p1_ddd_deviations not set: decision CR-DDD-01 / CR-DDD-04 not recorded';
    END IF;
END
$$;
