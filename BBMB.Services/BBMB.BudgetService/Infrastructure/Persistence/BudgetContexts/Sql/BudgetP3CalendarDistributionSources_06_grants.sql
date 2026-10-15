-- BudgetP3CalendarDistributionSources step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3).
-- T-20 and T-39 are mutable (SELECT/INSERT/UPDATE); T-18 is append-only (SELECT/INSERT, no UPDATE); no DELETE
-- on any table. bud_report reads the new tables and V-05, and V-04 (already granted) exposes the new columns.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.calendar, budget.distribution TO bud_app;
        GRANT SELECT, INSERT ON budget.expenditure_source_setting TO bud_app;
        GRANT SELECT ON budget.v_distribution_balance TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.calendar, budget.distribution, budget.expenditure_source_setting,
                        budget.v_distribution_balance TO bud_report;
    END IF;
END
$$;
