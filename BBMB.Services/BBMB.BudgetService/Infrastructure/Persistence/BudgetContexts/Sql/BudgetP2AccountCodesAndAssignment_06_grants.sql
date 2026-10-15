-- BudgetP2AccountCodesAndAssignment step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3, §8.3).
-- bud_app: SELECT, INSERT, UPDATE on T-21 (removal is an UPDATE; no DELETE on any table); T-07 UPDATE by column
-- (not_for_use and its three companions, party_emp_required). bud_fa: SELECT on V-07 only.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT, UPDATE ON budget.account_head_assignment TO bud_app;
        GRANT UPDATE (not_for_use, not_for_use_ref, not_for_use_at, not_for_use_by, party_emp_required,
                      modification_date, user_code) ON budget.budget_head_master TO bud_app;
        GRANT SELECT ON budget.v_manual_of_accounts_codes, budget.v_fa_account_code TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_fa') THEN
        GRANT USAGE ON SCHEMA budget TO bud_fa;
        GRANT SELECT ON budget.v_fa_account_code TO bud_fa;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.account_head_assignment, budget.v_manual_of_accounts_codes, budget.v_fa_account_code TO bud_report;
    END IF;
END
$$;
