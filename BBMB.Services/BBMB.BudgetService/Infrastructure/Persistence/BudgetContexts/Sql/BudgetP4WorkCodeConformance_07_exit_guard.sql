-- BudgetP4WorkCodeConformance step 6 (design §4.6 step 6): the exit guard, inside the migration. Any mismatch
-- raises and rolls the WHOLE migration back, so an incomplete or lossy reset cannot be committed. This is the
-- in-migration counterpart of the P4-02 preservation tests.
DO $$
DECLARE
    v_pre        bigint;
    v_prev       bigint;
    v_audit      bigint;
    v_mismatch   bigint;
    v_unmatched  bigint;
BEGIN
    SELECT works INTO v_pre FROM p4_pre;

    -- (a) Every work has its old code preserved in work_code_prev. No row lost.
    SELECT count(*) INTO v_prev FROM budget.work WHERE work_code_prev IS NOT NULL;
    IF v_prev <> v_pre THEN
        RAISE EXCEPTION 'work_code_prev count % <> pre-migration work count %', v_prev, v_pre;
    END IF;

    -- (b) One T-51 row of THIS transaction per work for the work_code reset (old -> null). The migration
    --     actor's request_id scopes the rows to this run.
    SELECT count(*) INTO v_audit
    FROM budget.audit_log
    WHERE table_name = 'work'
      AND column_name = 'work_code'
      AND action_code = 'MIGRATION:BudgetP4WorkCodeConformance'
      AND request_id = current_setting('budget.request_id', true)::uuid;
    IF v_audit <> v_pre THEN
        RAISE EXCEPTION 'audit rows for the work_code reset % <> pre-migration work count %', v_audit, v_pre;
    END IF;

    -- (c) Estimate linkage: every estimate's work_id resolves to a work whose preserved code equals the
    --     estimate's own retained work_code, and no estimate is left unlinked.
    SELECT count(*) INTO v_mismatch
    FROM budget.estimate e JOIN budget.work w ON w.id = e.work_id
    WHERE w.work_code_prev <> e.work_code;
    IF v_mismatch <> 0 THEN
        RAISE EXCEPTION 'estimate linkage mismatch: % estimate(s) point at a work whose preserved code differs', v_mismatch;
    END IF;

    SELECT count(*) INTO v_unmatched FROM budget.estimate WHERE work_id IS NULL;
    IF v_unmatched <> 0 THEN
        RAISE EXCEPTION 'estimate linkage: % estimate(s) have a null work_id after the reset', v_unmatched;
    END IF;
END
$$;
