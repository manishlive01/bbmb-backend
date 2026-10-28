-- BudgetP4AllocationProvisionFreeze step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3).
-- The new process tables are app-written; the append-only ones take SELECT/INSERT, the mutable ones
-- (provision supersession, estimate_allocation amount, unforeseen_request status) SELECT/INSERT/UPDATE; no
-- DELETE on any table. V-09 v_fa_provision is the Financial Accounting interface view, granted to bud_fa
-- (DDD §3.3: bud_fa has SELECT on V-07..V-09 and no direct privilege on any process table). bud_report reads
-- the new tables and V-02.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT ON budget.allocation, budget.provision_movement, budget.freeze,
                                budget.unforeseen_request_movement, budget.unforeseen_authorisation TO bud_app;
        GRANT SELECT, INSERT, UPDATE ON budget.provision, budget.estimate_allocation,
                                        budget.unforeseen_request TO bud_app;
        GRANT SELECT ON budget.v_provision_balance TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_fa') THEN
        GRANT SELECT ON budget.v_fa_provision TO bud_fa;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.allocation, budget.provision, budget.provision_movement,
                        budget.estimate_allocation, budget.freeze, budget.unforeseen_request,
                        budget.unforeseen_request_movement, budget.unforeseen_authorisation,
                        budget.v_provision_balance TO bud_report;
    END IF;
END
$$;
