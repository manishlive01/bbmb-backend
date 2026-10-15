-- BudgetP2AccountCodesAndAssignment step 1: guard (design §11.1 rule 4). Before any CHECK or FK is added, every
-- existing segment code must have the DDD T-01 to T-06 form and every T-07 row must find its two composite
-- parents (T-02 (major, sub-major), T-04 (minor, sub-minor)). Otherwise the migration aborts listing table,
-- column and count, the database unchanged; the owner corrects the rows under change control (DDD §9 L-04).
DO $$
DECLARE
    v_problems text := '';
    v_n        bigint;
BEGIN
    SELECT count(*) INTO v_n FROM budget.fa_major_head WHERE major_acc_code !~ '^\d{4}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_major_head.major_acc_code not ^\\d{4}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.fa_sub_major_head WHERE sub_major_acc_code !~ '^\d{2}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_sub_major_head.sub_major_acc_code not ^\\d{2}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.fa_minor_head WHERE minor_acc_code !~ '^\d{3}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_minor_head.minor_acc_code not ^\\d{3}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.fa_sub_minor_head WHERE sub_minor_acc_code !~ '^\d{2}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_sub_minor_head.sub_minor_acc_code not ^\\d{2}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.fa_detail_head WHERE detail_acc_code !~ '^\d{3}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_detail_head.detail_acc_code not ^\\d{3}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.fa_object_head WHERE object_acc_code !~ '^\d{3}$';
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  fa_object_head.object_acc_code not ^\\d{3}$: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.budget_head_master h
    WHERE NOT EXISTS (SELECT 1 FROM budget.fa_sub_major_head s
                      WHERE s.major_acc_code = h.major_acc_code AND s.sub_major_acc_code = h.sub_major_acc_code);
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  budget_head_master (major, sub_major) not in fa_sub_major_head: %s row(s)', v_n); END IF;
    SELECT count(*) INTO v_n FROM budget.budget_head_master h
    WHERE NOT EXISTS (SELECT 1 FROM budget.fa_sub_minor_head s
                      WHERE s.minor_acc_code = h.minor_acc_code AND s.sub_minor_acc_code = h.sub_minor_acc_code);
    IF v_n > 0 THEN v_problems := v_problems || format(E'\n  budget_head_master (minor, sub_minor) not in fa_sub_minor_head: %s row(s)', v_n); END IF;
    IF v_problems <> '' THEN
        RAISE EXCEPTION 'BudgetP2AccountCodesAndAssignment aborted: rows that do not meet DDD T-01 to T-07:%', v_problems;
    END IF;
END
$$;
