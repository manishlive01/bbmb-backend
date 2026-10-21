-- BudgetP6Reporting step 6: conditional grants (design §11.1 rule 7, OI-03; §8.8 bud_report). The reporting
-- role reads the reporting view; the application role reads it too (the query builder runs as the app).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT ON budget.v_report_demand TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.v_report_demand TO bud_report;
    END IF;
END
$$;
