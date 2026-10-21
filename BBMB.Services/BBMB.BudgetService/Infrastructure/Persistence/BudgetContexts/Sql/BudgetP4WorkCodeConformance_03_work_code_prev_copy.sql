-- BudgetP4WorkCodeConformance step 3 (design §4.6 step 3, §5.5 row b): copy every existing integer work
-- code into work_code_prev BEFORE the live work_code is reset. Runs under the migration actor (the
-- MigrationActor.sql fragment is executed immediately before this file), so work carries fn_require_actor
-- and fn_audit_row (from P1): fn_audit_row writes exactly one T-51 row per work for work_code_prev, with the
-- gate's recorded decision as budget.justification. NO existing value is lost — this is the independent
-- preservation record proved by P4-02 before P4 is marked done.
UPDATE budget.work SET work_code_prev = work_code;
