-- BudgetP4WorkCodeConformance step 4, pre-ALTER (design §4.6 step 4; §11.1 rule 2). The int -> varchar(12)
-- change of work.work_code cannot run while a view depends on the column (PostgreSQL 0A000, "cannot alter
-- type of a column used by a view or rule"). The only dependant is V-08 budget.v_fa_work_sanction (P2), which
-- exposes w.work_code to Financial Accounting. It is dropped here and recreated unchanged after the type
-- change (step 4 recreate), so the FA-facing shape is identical except that work_code is now varchar(12).
DROP VIEW IF EXISTS budget.v_fa_work_sanction;
