-- BudgetP2AttachmentsApprovalsSanctions step 6: conditional grants (design §11.1 rule 7, OI-03; DDD §3.3).
-- No UPDATE on append-only tables, no DELETE on any table. bud_fa: SELECT on V-08 and EXECUTE on
-- fn_record_estimate_code only, no direct privilege on a process table.
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_app') THEN
        GRANT SELECT, INSERT ON budget.attachment, budget.delegation_limit,
                                budget.admin_approval_version, budget.technical_sanction_version TO bud_app;
        GRANT SELECT, INSERT, UPDATE ON budget.admin_approval, budget.correction_flag TO bud_app;
        GRANT SELECT, INSERT, UPDATE (current_version_no) ON budget.technical_sanction TO bud_app;
        GRANT SELECT ON budget.v_demand_line_display, budget.v_fa_work_sanction TO bud_app;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_fa') THEN
        GRANT USAGE ON SCHEMA budget TO bud_fa;
        GRANT SELECT ON budget.v_fa_work_sanction TO bud_fa;
        GRANT EXECUTE ON FUNCTION budget.fn_record_estimate_code(bigint, varchar) TO bud_fa;
    END IF;
    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'bud_report') THEN
        GRANT SELECT ON budget.attachment, budget.delegation_limit, budget.admin_approval,
                        budget.admin_approval_version, budget.technical_sanction, budget.technical_sanction_version,
                        budget.correction_flag, budget.v_demand_line_display, budget.v_fa_work_sanction TO bud_report;
    END IF;
END
$$;
