-- BudgetP1StateTransitionSeed step 5: conditional grants (design §11.1 rule 7, OI-03).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT ON budget.state_transition TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.state_transition TO bud_report;
    END IF;
END
$$;
