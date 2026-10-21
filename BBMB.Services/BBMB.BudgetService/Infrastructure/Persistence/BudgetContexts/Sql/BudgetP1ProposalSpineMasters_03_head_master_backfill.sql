-- BudgetP1ProposalSpineMasters step 3 (design §5.2 T-07 order): audited backfills under the migration
-- actor (the fragment runs first). Every existing row was loaded from the Manual in use (DDD §9 L-04), so
-- origin = LEGACY; party_emp_required, where null, by the BUD-267 rule (Y for majors 8443 and 9906, else
-- N; OI-17). account_code_id and complete_code are derived by DDL and have no T-51 row.
UPDATE budget.budget_head_master SET origin = 'LEGACY' WHERE origin IS NULL;

UPDATE budget.budget_head_master
SET party_emp_required = CASE WHEN major_acc_code IN ('8443', '9906') THEN 'Y' ELSE 'N' END
WHERE party_emp_required IS NULL;
