-- BudgetP1ProposalSpineMasters step 1 (design §5.2, §11.1 rule 4): T-07 sub_major_acc_code
-- varchar(4) -> varchar(2). Aborts, listing the rows, where a value is longer than 2: narrowing would
-- otherwise fail or lose data. The rows are corrected by their owner under change control.
DO $$
DECLARE
    v_list text;
BEGIN
    SELECT string_agg(format('%s-%s-%s-%s-%s-%s', major_acc_code, sub_major_acc_code, minor_acc_code,
                             sub_minor_acc_code, detail_acc_code, object_acc_code), '; ')
    INTO v_list
    FROM budget.budget_head_master
    WHERE length(sub_major_acc_code) > 2;
    IF v_list IS NOT NULL THEN
        RAISE EXCEPTION 'BudgetP1ProposalSpineMasters aborted: budget_head_master rows with sub_major_acc_code longer than 2: %', v_list;
    END IF;
END
$$;
