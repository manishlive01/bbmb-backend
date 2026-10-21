-- BudgetP1ProposalSpine step 5: DDD §3.3 privileges, conditional on the roles existing (design §11.1 rule 7, OI-03).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.proposal, budget.demand_line, budget.observation TO bud_app;
        GRANT SELECT, INSERT ON budget.proposal_movement TO bud_app;
        GRANT SELECT ON budget.figure_version TO bud_app;
        GRANT EXECUTE ON FUNCTION budget.fn_capture_figure_version() TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.proposal, budget.demand_line, budget.figure_version, budget.observation,
                        budget.proposal_movement TO bud_report;
    END IF;
END
$$;
