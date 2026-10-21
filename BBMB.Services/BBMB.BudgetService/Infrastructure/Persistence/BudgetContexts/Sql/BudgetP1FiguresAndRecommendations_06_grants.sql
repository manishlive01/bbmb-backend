-- BudgetP1FiguresAndRecommendations step 6: conditional grants (design §11.1 rule 7, OI-03).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.opening_figure, budget.ce_recommendation TO bud_app;
        GRANT SELECT, INSERT ON budget.expenditure_figure TO bud_app;
        GRANT SELECT ON budget.recommendation_version, budget.v_demand_by_ce, budget.v_demand_line_display TO bud_app;
        GRANT EXECUTE ON FUNCTION budget.fn_capture_recommendation_version() TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.expenditure_figure, budget.opening_figure, budget.ce_recommendation,
                        budget.recommendation_version, budget.v_demand_by_ce, budget.v_demand_line_display TO bud_report;
    END IF;
END
$$;
