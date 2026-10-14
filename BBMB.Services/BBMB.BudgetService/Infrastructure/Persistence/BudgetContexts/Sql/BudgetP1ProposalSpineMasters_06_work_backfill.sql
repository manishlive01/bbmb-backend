-- BudgetP1ProposalSpineMasters step 6 (design §5.2 work order, step 3; §5.5 row a): every work that exists
-- before the DDD is marked pre_ddd, under the migration actor (audited: one T-51 row per work for pre_ddd).
-- The creation columns are not touched: an old work's creator was never recorded, and stamping the
-- migration actor would be a false attribution (ALCOA+, BUD-036).
UPDATE budget.work SET pre_ddd = true;
