-- BudgetP4WorkCodeConformance step 2 (design §4.6 step 2): once the step-1 guard has passed, every
-- estimate.work_id is set, so the column can be made NOT NULL. Placed by hand (it replaces the NOT NULL the
-- generated AddColumn would have carried, which was edited to nullable so the guard could run). This must
-- precede the AddPrimaryKey(work_id, estimate_code) EF op (a PK column must be NOT NULL).
ALTER TABLE budget.estimate ALTER COLUMN work_id SET NOT NULL;
