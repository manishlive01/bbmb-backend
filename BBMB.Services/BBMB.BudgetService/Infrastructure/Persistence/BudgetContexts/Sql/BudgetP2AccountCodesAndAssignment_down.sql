-- Down of BudgetP2AccountCodesAndAssignment, before EF drops T-21 and narrows the descriptions. The narrowing is
-- refused when a description no longer fits in 100 characters (no silent truncation).
DO $$
DECLARE
    v_n bigint;
BEGIN
    SELECT (SELECT count(*) FROM budget.fa_major_head WHERE length(major_acc_desc) > 100)
         + (SELECT count(*) FROM budget.fa_sub_major_head WHERE length(sub_major_acc_desc) > 100)
         + (SELECT count(*) FROM budget.fa_minor_head WHERE length(minor_acc_desc) > 100)
         + (SELECT count(*) FROM budget.fa_sub_minor_head WHERE length(sub_minor_acc_desc) > 100)
         + (SELECT count(*) FROM budget.fa_detail_head WHERE length(detail_acc_desc) > 100) INTO v_n;
    IF v_n > 0 THEN
        RAISE EXCEPTION 'BudgetP2AccountCodesAndAssignment Down refused: % segment description(s) longer than 100 characters', v_n;
    END IF;
END
$$;
DROP VIEW budget.v_fa_account_code;
DROP VIEW budget.v_manual_of_accounts_codes;
DROP TRIGGER trg_demand_line_head_assigned ON budget.demand_line;
DROP TRIGGER trg_work_head_not_for_use ON budget.work;
DROP TRIGGER trg_head_master_protect ON budget.budget_head_master;
DROP TRIGGER trg_account_head_assignment_rules ON budget.account_head_assignment;
DROP TRIGGER trg_account_head_assignment_forbid_stmt ON budget.account_head_assignment;
DROP TRIGGER trg_account_head_assignment_audit ON budget.account_head_assignment;
DROP TRIGGER trg_account_head_assignment_require_actor ON budget.account_head_assignment;
DROP FUNCTION budget.fn_demand_line_head_assigned();
DROP FUNCTION budget.fn_account_head_assignment_rules();
DROP FUNCTION budget.fn_work_head_not_for_use();
DROP FUNCTION budget.fn_head_master_protect();
DROP TRIGGER trg_fa_major_head_forbid_row ON budget.fa_major_head;
DROP TRIGGER trg_fa_sub_major_head_forbid_row ON budget.fa_sub_major_head;
DROP TRIGGER trg_fa_minor_head_forbid_row ON budget.fa_minor_head;
DROP TRIGGER trg_fa_sub_minor_head_forbid_row ON budget.fa_sub_minor_head;
DROP TRIGGER trg_fa_detail_head_forbid_row ON budget.fa_detail_head;
DROP TRIGGER trg_fa_object_head_forbid_row ON budget.fa_object_head;
ALTER TABLE budget.budget_head_master
    DROP CONSTRAINT fk_budget_head_master_sub_major,
    DROP CONSTRAINT fk_budget_head_master_sub_minor;
ALTER TABLE budget.fa_major_head     DROP CONSTRAINT ck_fa_major_head_code;
ALTER TABLE budget.fa_sub_major_head DROP CONSTRAINT ck_fa_sub_major_head_code;
ALTER TABLE budget.fa_minor_head     DROP CONSTRAINT ck_fa_minor_head_code;
ALTER TABLE budget.fa_sub_minor_head DROP CONSTRAINT ck_fa_sub_minor_head_code;
ALTER TABLE budget.fa_detail_head    DROP CONSTRAINT ck_fa_detail_head_code;
ALTER TABLE budget.fa_object_head    DROP CONSTRAINT ck_fa_object_head_code;
