-- BudgetP5Reappropriation step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3). T-46 and T-47 are
-- app-written and mutable (SELECT/INSERT/UPDATE); no DELETE. bud_report reads the new tables and V-10.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.reappropriation, budget.reappropriation_source TO bud_app;
        GRANT SELECT ON budget.v_reappropriation_register TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.reappropriation, budget.reappropriation_source,
                        budget.v_reappropriation_register TO bud_report;
    END IF;
END
$$;
