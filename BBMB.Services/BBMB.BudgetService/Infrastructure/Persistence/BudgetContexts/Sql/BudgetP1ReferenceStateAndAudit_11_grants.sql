-- BudgetP1ReferenceStateAndAudit step 11: privileges of DDD §3.3, design §11.1 rule 7. Granted only if
-- the role exists: creating the roles and moving the application login to bud_app is a deployment step
-- (OI-03). Until then the triggers are the only control (design §4.1).
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.office, budget.budget_cycle TO bud_app;
        GRANT SELECT, INSERT ON budget.state, budget.setting TO bud_app;
        GRANT SELECT ON budget.audit_log TO bud_app;
        GRANT EXECUTE ON FUNCTION budget.fn_require_actor(), budget.fn_audit_row(), budget.fn_forbid_change() TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.office, budget.budget_cycle, budget.state, budget.setting TO bud_report;
    END IF;
END
$$;
